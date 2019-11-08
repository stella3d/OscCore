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
    public sealed unsafe class OscServer : IDisposable
    {
        readonly Socket m_Socket;
        readonly Thread m_Thread;
        bool m_Disposed;
        bool m_Started;

        readonly byte[] m_ReadBuffer;
        GCHandle m_BufferHandle;
        byte* m_BufferPtr;
        
        Action[] m_MainThreadQueue = new Action[16];
        int m_MainThreadCount;

        readonly Dictionary<int, string> m_ByteLengthToStringBuffer = new Dictionary<int, string>();
        
        readonly HashSet<MonitorCallback> m_MonitorCallbacks = new HashSet<MonitorCallback>();
        
        readonly List<OscActionPair> m_PatternMatchedMethods = new List<OscActionPair>();
        
        public static readonly Dictionary<int, OscServer> PortToServer = new Dictionary<int, OscServer>();

        public int Port { get; }
        public OscAddressSpace AddressSpace { get; private set; }
        public OscParser Parser { get; }
        
        public OscServer(int port, int bufferSize = 4096)
        {
            if (PortToServer.ContainsKey(port))
            {
                Debug.LogError($"port {port} is already in use, cannot start a new OSC Server on it");
                return;
            }

            k_SingleCallbackToPair.Clear();
            AddressSpace = new OscAddressSpace();
            
            m_ReadBuffer = new byte[bufferSize];
            m_BufferHandle = GCHandle.Alloc(m_ReadBuffer, GCHandleType.Pinned);
            m_BufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();
            Parser = new OscParser(m_ReadBuffer);

            Port = port;
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = int.MaxValue };
            m_Thread = new Thread(Serve);
            Start();
        }

        public void Start()
        {
            // make sure redundant calls don't do anything after the first
            if (m_Started) return;
            
            m_Disposed = false;
            if (!m_Socket.IsBound)
                m_Socket.Bind(new IPEndPoint(IPAddress.Any, Port));

            m_Thread.Start();
            m_Started = true;
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
        
        // used to allow easy removal of single callbacks
        static readonly Dictionary<Action<OscMessageValues>, OscActionPair> k_SingleCallbackToPair = 
            new Dictionary<Action<OscMessageValues>, OscActionPair>();
        
        /// <summary>
        /// Register a single background thread method for an OSC address
        /// </summary>
        /// <param name="address">The OSC address to handle messages for</param>
        /// <param name="valueReadMethod">
        /// The method to execute immediately on the worker thread that reads values from the message
        /// </param>
        /// <returns>True if the address was valid, false otherwise</returns>
        public bool TryAddMethod(string address, Action<OscMessageValues> valueReadMethod)
        {
            var pair = new OscActionPair(valueReadMethod);
            k_SingleCallbackToPair.Add(valueReadMethod, pair);
            return AddressSpace.TryAddMethod(address, pair);
        }
        
        /// <summary>
        /// Remove a single background thread method from an OSC address
        /// </summary>
        /// <param name="address">The OSC address to handle messages for</param>
        /// <param name="valueReadMethod">
        /// The method to execute immediately on the worker thread that reads values from the message
        /// </param>
        /// <returns>True if the method was removed from this address, false otherwise</returns>
        public bool RemoveMethod(string address, Action<OscMessageValues> valueReadMethod)
        {
            if (k_SingleCallbackToPair.TryGetValue(valueReadMethod, out var pair))
            {
                return AddressSpace.RemoveMethod(address, pair) && 
                        k_SingleCallbackToPair.Remove(valueReadMethod);
            }

            return false;
        }

        /// <summary>
        /// Add a background thread read callback and main thread callback associated with an OSC address.
        /// </summary>
        /// <param name="address">The OSC address to associate a method with</param>
        /// <param name="actionPair">The pair of callbacks to add</param>
        /// <returns>True if the address was valid & methods associated with it, false otherwise</returns>
        public bool TryAddMethodPair(string address, OscActionPair actionPair) => 
            AddressSpace.TryAddMethod(address, actionPair);
        
        /// <summary>
        /// Remove a background thread read callback and main thread callback associated with an OSC address.
        /// </summary>
        /// <param name="address">The OSC address to remove methods from</param>
        /// <param name="actionPair">The pair of callbacks to remove</param>
        /// <returns>True if successfully removed, false otherwise</returns>
        public bool RemoveMethodPair(string address, OscActionPair actionPair)
        {
            // if the address space is null, this got called during cleanup / shutdown,
            // and effectively all addresses are removed by setting it to null
            return AddressSpace == null || AddressSpace.RemoveMethod(address, actionPair);
        }

        /// <summary>
        /// Add a method to be invoked every time an OSC message is received. If there are any monitor callbacks added,
        /// memory has to be allocated for every message received, so it's recommended to only do this while editing.
        /// </summary>
        /// <param name="callback">The method to invoke</param>
        public void AddMonitorCallback(MonitorCallback callback)
        {
            m_MonitorCallbacks.Add(callback);
        }
        
        /// <summary>Remove a monitor method</summary>
        /// <param name="callback">The method to remove</param>
        public bool RemoveMonitorCallback(MonitorCallback callback)
        {
            return m_MonitorCallbacks.Remove(callback);
        }

        /// <summary>Must be called on the main thread every frame to handle queued events</summary>
        public void Update()
        {
            for (int i = 0; i < m_MainThreadCount; i++)
            {
                m_MainThreadQueue[i]();
            }
            
            m_MainThreadCount = 0;
        }

        void Serve()
        {
#if OSCCORE_PROFILING && UNITY_EDITOR
            Profiler.BeginThreadProfiling("OscCore", "Server");
#endif
            var buffer = m_ReadBuffer;
            var bufferPtr = Parser.BufferPtr;
            var bufferLongPtr = Parser.BufferLongPtr;
            var parser = Parser;
            var addressToMethod = AddressSpace.AddressToMethod;
            var socket = m_Socket;
            
            while (!m_Disposed)
            {
                try
                {
                    // it's probably better to let Receive() block the thread than test socket.Available > 0 constantly
                    int receivedByteCount = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (receivedByteCount == 0) continue;

                    Profiler.BeginSample("Receive OSC");
                    // determine if the message is a bundle or not 
                    if (*bufferLongPtr != Constant.BundlePrefixLong)
                    {
                        // address length here doesn't include the null terminator and alignment padding.
                        // this is so we can look up the address by only its content bytes.
                        var addressLength = parser.FindAddressLength();
                        if (addressLength < 0)
                        {
                            // address didn't start with '/'
                            Profiler.EndSample();
                            continue;
                        }

                        var alignedAddressLength = (addressLength + 3) & ~3;
                        // if the null terminator after the string comes at the beginning of a 4-byte block,
                        // we need to add 4 bytes of padding
                        if (alignedAddressLength == addressLength)
                            alignedAddressLength += 4;

                        var tagCount = parser.ParseTags(buffer, alignedAddressLength);
                        if (tagCount <= 0)
                        {
                            Profiler.EndSample();
                            continue;
                        }

                        var offset = alignedAddressLength + (tagCount + 4) & ~3;
                        parser.FindOffsets(offset);

                        // see if we have a method registered for this address
                        if (addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var methodPair))
                        {
                            // call the value read method associated with this OSC address    
                            methodPair.ValueRead(parser.MessageValues);
                            // if there's a main thread method, queue it
                            if (methodPair.MainThreadQueued != null)
                            {
                                if (m_MainThreadCount >= m_MainThreadQueue.Length)
                                    Array.Resize(ref m_MainThreadQueue, m_MainThreadQueue.Length * 2);

                                m_MainThreadQueue[m_MainThreadCount++] = methodPair.MainThreadQueued;
                            }
                        }
                        else if (AddressSpace.PatternCount > 0)
                        {
                            TryMatchPatterns(parser, bufferPtr, addressLength);
                        }

                        Profiler.EndSample();

                        if (m_MonitorCallbacks.Count == 0) continue;
                        
                        // handle monitor callbacks
                        var monitorAddressStr = new BlobString(bufferPtr, addressLength);
                        foreach (var callback in m_MonitorCallbacks)
                            callback(monitorAddressStr, parser.MessageValues);

                        continue;
                    }

                    // the message is a bundle, so we need to recursively scan the bundle elements
                    int MessageOffset = 0;
                    bool recurse;
                    // the outer do-while loop runs once for every #bundle encountered
                    do
                    {
                        // Timestamp isn't used yet, but it will be eventually
                        // var time = parser.MessageValues.ReadTimestampIndex(MessageOffset + 8);
                        // '#bundle ' + timestamp = 16 bytes
                        MessageOffset += 16;
                        recurse = false;

                        // the inner while loop runs once per bundle element
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
                                if (bundleMethodPair.MainThreadQueued != null)
                                {
                                    if (m_MainThreadCount >= m_MainThreadQueue.Length)
                                        Array.Resize(ref m_MainThreadQueue, m_MainThreadQueue.Length * 2);

                                    m_MainThreadQueue[m_MainThreadCount++] = bundleMethodPair.MainThreadQueued;
                                }
                            }
                            // if we have no handler for this exact address, we may have a pattern that matches it
                            else if (AddressSpace.PatternCount > 0)
                            {
                                TryMatchPatterns(parser, bufferPtr, bundleAddressLength);
                            }

                            MessageOffset += messageSize + 4;

                            if (m_MonitorCallbacks.Count == 0) continue;

                            var bundleMemberAddressStr = new BlobString(bufferPtr + contentIndex, bundleAddressLength);
                            foreach (var callback in m_MonitorCallbacks)
                                callback(bundleMemberAddressStr, parser.MessageValues);
                        }
                    }
                    // restart the outer while loop every time a bundle within a bundle is detected
                    while (recurse);
                    Profiler.EndSample();
                }
                // a read timeout can result in a socket exception, should just be ok to ignore
                catch (SocketException) { }
                catch (ThreadAbortException) {}
                catch (Exception e)
                {
                    if (!m_Disposed) Debug.LogException(e); 
                    break;
                }
            }
            
            Profiler.EndThreadProfiling();
        }
        
        void TryMatchPatterns(OscParser parser, byte* bufferPtr, int addressLength)
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
        static void OverwriteAsciiString(string str, byte* bufferPtr)
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

            PortToServer.Remove(Port);

            if(m_BufferHandle.IsAllocated) m_BufferHandle.Free();
            if (disposing)
            {
                AddressSpace.AddressToMethod.Dispose();
                AddressSpace = null;
                
                m_Socket.Close();
                m_Socket.Dispose();
            }
        }

        ~OscServer()
        {
            Dispose(true);
        }
    }
}