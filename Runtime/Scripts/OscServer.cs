using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BlobHandles;
using UnityEngine;
using UnityEngine.Profiling;

namespace OscCore
{
    public sealed class OscServer : IDisposable
    {
        public static readonly Dictionary<int, OscServer> PortToServer = new Dictionary<int, OscServer>();
        
        readonly Socket m_Socket;
        readonly Thread m_Thread;
        bool m_Disposed;

        readonly byte[] m_ReadBuffer;
        GCHandle m_BufferHandle;
        
        Action[] m_MainThreadQueue = new Action[512];
        int m_MainThreadCount;

        readonly Dictionary<int, string> m_ByteLengthToStringBuffer = new Dictionary<int, string>();
        
        readonly List<MonitorCallback> m_MonitorCallbacks = new List<MonitorCallback>();
        
        readonly List<OscActionPair> m_PatternMatchedMethods = new List<OscActionPair>();

        public int Port { get; private set; }
        public OscAddressSpace AddressSpace { get; private set; }
        public OscParser Parser { get; private set; }
        
        public OscServer(int port, int bufferSize = 4096)
        {
            if (PortToServer.ContainsKey(port))
            {
                Debug.LogError($"port {port} is already in use, cannot start an OSC Server on it");
                return;
            }

            AddressSpace = new OscAddressSpace();
            
            m_ReadBuffer = new byte[bufferSize];
            m_BufferHandle = GCHandle.Alloc(m_ReadBuffer, GCHandleType.Pinned);
            Parser = new OscParser(m_ReadBuffer, m_BufferHandle);

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = 64 };
            m_Thread = new Thread(Serve);
            Port = port;
        }

        public void Start()
        {
            m_Disposed = false;
            m_Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            m_Thread.Start();
        }
        
        public void Pause()
        {
            m_Disposed = true;
        }
        
        public void Resume()
        {
            m_Disposed = false;
        }
        
        public static OscServer GetOrCreate(int port)
        {
            OscServer server;
            if (!PortToServer.TryGetValue(port, out server))
            {
                server = new OscServer(port);
                PortToServer[port] = server;
            }
            return server;
        }
        
        public static bool Remove(int port)
        {
            OscServer server;
            if (PortToServer.TryGetValue(port, out server))
            {
                server.Dispose();
                return PortToServer.Remove(port);
            }
            return false;
        }

        public bool TryAddMethod(string address, OscActionPair method) => 
            AddressSpace.TryAddMethod(address, method);
        
        public bool RemoveMethod(string address, OscActionPair method) => 
            AddressSpace.RemoveMethod(address, method);

        /// <summary>
        /// Add a method to be invoked every time an OSC message is received. If there are any monitor callbacks added,
        /// memory has to be allocated for every message received, so it's recommended to only do this while editing.
        /// </summary>
        /// <param name="callback">The method to invoke</param>
        public void AddMonitorCallback(MonitorCallback callback)
        {
            if (!m_MonitorCallbacks.Contains(callback)) 
                m_MonitorCallbacks.Add(callback);
        }
        
        /// <summary>Remove a monitor method</summary>
        /// <param name="callback">The method to remove</param>
        public bool RemoveMonitorCallback(MonitorCallback callback)
        {
            return m_MonitorCallbacks.Remove(callback);
        }

        public void Update()
        {
            try
            {
                for (int i = 0; i < m_MainThreadCount; i++)
                {
                    m_MainThreadQueue[i]();
                }
            }
            catch (Exception e) { }
            m_MainThreadCount = 0;
        }

        unsafe void Serve()
        {
#if OSCCORE_PROFILING && UNITY_EDITOR
            Profiler.BeginThreadProfiling("OscCore", "Server");
#endif
            var socket = m_Socket;
            var buffer = m_ReadBuffer;
            var bufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();
            var bufferLongPtr = Parser.BufferLongPtr;
            var parser = Parser;
            var addressToMethod = AddressSpace.AddressToMethod;

            while (!m_Disposed)
            {
                try
                {
                    // it's probably better to let Receive() block the thread than test socket.Available > 0 constantly
                    int receivedByteCount = socket.Receive(buffer);
                    if (receivedByteCount == 0) continue;
                    
#if OSCCORE_PROFILING && UNITY_EDITOR
                    Profiler.BeginSample("Receive OSC");
#endif
                    // compare the first 8 bytes at once, against '#bundle ' represented as a long   
                    if (*bufferLongPtr != Constant.BundlePrefixLong)
                    {
                         // The message isn't a bundle
                        var addressLength = parser.FindAddressLength();
                        if (addressLength < 0) continue;                         // address didn't start with '/'

                        var tagCount = parser.ParseTags(buffer, addressLength);
                        if (tagCount <= 0) continue;
                        
                        var offset = addressLength + (tagCount + 4) & ~3;
                        parser.FindOffsets(offset);
                        
                        if (addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var methodPair))
                        {
                            // call the value read method associated with this OSC address    
                            methodPair.ValueRead(parser.MessageValues);
                            // if there's a main thread method, queue it
                            if(methodPair.MainThreadQueued != null)    
                                m_MainThreadQueue[m_MainThreadCount++] = methodPair.MainThreadQueued;
                        }
                        else if(AddressSpace.PatternCount > 0)
                        {
                            TryMatchPatterns(parser, bufferPtr, addressLength);
                        }
#if OSCCORE_PROFILING && UNITY_EDITOR
                        Profiler.EndSample();
#endif                        
                        if (m_MonitorCallbacks.Count == 0) continue;

                        var monitorAddressStr = new BlobString(bufferPtr, addressLength);
                        foreach (var callback in m_MonitorCallbacks)
                            callback(monitorAddressStr, parser.MessageValues);

                        continue;
                    }
                    
                    // the message is a bundle, so we need to recursively scan the bundle elements
                    // '#bundle ' + timestamp = 16 bytes
                    int MessageOffset = 0;         
                    bool recurse;
                    do
                    {
                        // Timestamp isn't used yet, but it will be eventually
                        // var time = parser.MessageValues.ReadTimestampIndex(MessageOffset + 8);
                        MessageOffset += 16;
                        recurse = false;
                        
                        while (MessageOffset < receivedByteCount && !recurse)
                        {
                            var messageSize = (int) parser.MessageValues.ReadUIntIndex(MessageOffset);
                            var contentIndex = MessageOffset + 4;

                            if (parser.IsBundleTagAtIndex(contentIndex))
                            {
                                // this bundle element's contents are a bundle, break out to the outer loop to scan it
                                MessageOffset = contentIndex;
                                recurse = true;
                                continue;
                            }

                            var bundleAddressLength = parser.FindAddressLength(contentIndex);
                            if (bundleAddressLength <= 0)
                            {
                                // if an error occured parsing the address, skip this message entirely
                                MessageOffset += messageSize + 4;
                                continue;
                            }

                            var bundleTagCount = parser.ParseTags(buffer, contentIndex + bundleAddressLength);
                            if (bundleTagCount <= 0)
                            {
                                MessageOffset += messageSize + 4;
                                continue;
                            }

                            // skip the ',' and align to 4 bytes
                            var bundleOffset = (contentIndex + bundleAddressLength + bundleTagCount + 4) & ~3;
                            parser.FindOffsets(bundleOffset);

                            if (addressToMethod.TryGetValueFromBytes(bufferPtr + contentIndex, bundleAddressLength,
                                out var bundleMethodPair))
                            {
                                // call the value read method associated with this OSC address    
                                bundleMethodPair.ValueRead(parser.MessageValues);
                                // if there's a main thread method, queue it
                                if(bundleMethodPair.MainThreadQueued != null)    
                                    m_MainThreadQueue[m_MainThreadCount++] = bundleMethodPair.MainThreadQueued;
                            }
                            // if we have no handler for this exact address, we may have a pattern that matches it
                            else if (AddressSpace.PatternCount > 0)
                            {
                                TryMatchPatterns(parser, bufferPtr, bundleAddressLength);
                            }

                            MessageOffset += messageSize + 4;
                            
                            if (m_MonitorCallbacks.Count == 0) continue;

                            // this doesn't actually allocate a string unless the monitor callback converts to string
                            var bundleMemberAddressStr = new BlobString(bufferPtr + contentIndex, bundleAddressLength);
                            foreach (var callback in m_MonitorCallbacks) 
                                callback(bundleMemberAddressStr, parser.MessageValues);
                        }
                    } 
                    // restart the outer while loop every time a bundle within a bundle is detected
                    while (recurse);
                }
                catch (SocketException) {}
                catch (ThreadAbortException) {}
                catch (Exception e)
                {
                    if (!m_Disposed) Debug.LogException(e); 
                    break;
                }
            }
#if OSCCORE_PROFILING && UNITY_EDITOR   
            Profiler.EndThreadProfiling();
#endif
        }

        unsafe void TryMatchPatterns(OscParser parser, byte* bufferPtr, int addressLength)
        {
            // to support OSC address patterns, we test unmatched addresses against regular expressions
            // To do that, we need it as a regular string.  We may be able to mutate a previous string, 
            // instead of always allocating a new one
            if (!m_ByteLengthToStringBuffer.TryGetValue(addressLength, out var stringBuffer))
            {
                stringBuffer = Encoding.ASCII.GetString(bufferPtr, addressLength);
                m_ByteLengthToStringBuffer[addressLength] = stringBuffer;
            }
            else
            {
                // If we've previously received a message of the same byte length, we can re-use it
                OverwriteAsciiString(stringBuffer, bufferPtr);
            }

            if (AddressSpace.TryMatchPatternHandler(stringBuffer, m_PatternMatchedMethods))
            {
                var bufferCopy = string.Copy(stringBuffer);
                AddressSpace.AddressToMethod.Add(bufferCopy, m_PatternMatchedMethods);
                foreach (var matchedMethod in m_PatternMatchedMethods)
                {
                    matchedMethod.ValueRead(parser.MessageValues);
                    m_MainThreadQueue[m_MainThreadCount++] = matchedMethod.MainThreadQueued;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void OverwriteAsciiString(string str, byte* bufferPtr)
        {
            fixed (char* addressPtr = str)                    // done parsing this bundle message , wait for the next one
            {
                for (int i = 0; i < str.Length; i++)
                    addressPtr[i] = (char) bufferPtr[i];
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (m_Disposed) return;
            m_Disposed = true;

            if(m_BufferHandle.IsAllocated) m_BufferHandle.Free();
            if (disposing)
            {
                AddressSpace.AddressToMethod.Dispose();
                AddressSpace = null;
                if (m_Socket.IsBound)
                {
                    m_Socket.Close();
                    m_Socket.Dispose();
                }

                if(m_Thread.ThreadState == ThreadState.Running)
                    m_Thread.Join();
            }
        }

        ~OscServer()
        {
            Dispose(true);
        }
    }
}