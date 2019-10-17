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

        readonly Dictionary<int, string> m_ByteLengthToStringBuffer = new Dictionary<int, string>();
        
        readonly List<MonitorCallback> m_MonitorCallbacks = new List<MonitorCallback>();
        
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

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = 69 };
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

        public bool TryAddMethod(string address, ReceiveValueMethod method) => 
            AddressSpace.TryAddMethod(address, method);
        
        public bool RemoveMethod(string address, ReceiveValueMethod method) => 
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
        
        unsafe void Serve()
        {
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
                    int receivedByteCount = socket.Receive(buffer);
                    if (receivedByteCount <= 0) continue;

                    // determine if the message is a bundle by comparing the first 8 bytes of the buffer as a long
                    if (*bufferLongPtr == Constant.BundlePrefixLong)    
                    {
                        // Timestamp isn't used yet
                        // var time = parser.MessageValues.ReadTimestampIndex(8);

                        int MessageOffset = 16;    // 8 byte #bundle label + 8 byte timestamp
                        while (MessageOffset < receivedByteCount)
                        {
                            var messageSize = (int) parser.MessageValues.ReadUIntIndex(MessageOffset);
                            var contentIndex = MessageOffset + 4;

                            var bundleAddressLength = parser.FindAddressLength(contentIndex);
                            if (bundleAddressLength < 0)
                            {
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
                            var bundleOffset = (MessageOffset + bundleAddressLength + bundleTagCount + 4) & ~3;
                            parser.FindOffsets(bundleOffset);
                            
                            if (addressToMethod.TryGetValueFromBytes(bufferPtr, bundleAddressLength, out var bundleMethod))
                            {
                                // call the method(s) associated with this OSC address    
                                bundleMethod.Invoke(parser.MessageValues);
                            }
                            // if we have no handler for this exact address, we may have a pattern that matches it
                            else if(AddressSpace.PatternCount > 0)
                            {
                                if (!m_ByteLengthToStringBuffer.TryGetValue(bundleAddressLength, out var stringBuffer))
                                {
                                    // if we don't have an existing string of the right length to re-use, create a new one
                                    stringBuffer = Encoding.ASCII.GetString(bufferPtr + contentIndex, bundleAddressLength);
                                    m_ByteLengthToStringBuffer[bundleAddressLength] = stringBuffer;
                                }
                                else
                                {
                                    OverwriteAsciiString(stringBuffer, bufferPtr + MessageOffset);
                                }

                                // test the address against all registered address patterns for a method to invoke if matched
                                if (AddressSpace.TryMatchPatternHandler(stringBuffer, out bundleMethod))
                                {
                                    bundleMethod.Invoke(parser.MessageValues);
                                    // add the method found via pattern matching as a handler for this exact address
                                    // this means that next time a message is received at this address,
                                    // we don't need to run pattern matching again
                                    addressToMethod.Add(string.Copy(stringBuffer), bundleMethod);
                                }
                            }
                            
                            MessageOffset += messageSize + 4;
                        }

                        continue;
                    }

                    // The message isn't a bundle
                    var addressLength = parser.FindAddressLength();
                    if (addressLength < 0) continue;                         // address didn't start with '/'

                    var tagCount = parser.ParseTags(buffer, addressLength);
                    if (tagCount <= 0) continue;
                    
                    var offset = addressLength + (tagCount + 4) & ~3;
                    parser.FindOffsets(offset);
                    
                    if (addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var method))
                    {
                        // call the method(s) associated with this OSC address    
                        method.Invoke(parser.MessageValues);
                    }
                    else if(AddressSpace.PatternCount > 0)
                    {
                        if (!m_ByteLengthToStringBuffer.TryGetValue(addressLength, out var stringBuffer))
                        {
                            stringBuffer = Encoding.ASCII.GetString(bufferPtr, addressLength);
                            m_ByteLengthToStringBuffer[addressLength] = stringBuffer;
                        }
                        else
                        {
                            OverwriteAsciiString(stringBuffer, bufferPtr);
                        }

                        if (AddressSpace.TryMatchPatternHandler(stringBuffer, out method))
                        {
                            method.Invoke(parser.MessageValues);
                            addressToMethod.Add(string.Copy(stringBuffer), method);
                        }
                    }

                    if (m_MonitorCallbacks.Count == 0) continue;

                    var monitorAddressStr = new BlobString(bufferPtr, addressLength);
                    // call all monitor callbacks
                    foreach (var callback in m_MonitorCallbacks)
                        callback(monitorAddressStr, parser.MessageValues);
                }
                catch (SocketException) {}
                catch (ThreadAbortException) {}
                catch (Exception e)
                {
                    if (!m_Disposed) Debug.LogException(e); 
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void OverwriteAsciiString(string str, byte* bufferPtr)
        {
            fixed (char* addressPtr = str)
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
                AddressSpace = null;
                if(m_Socket.IsBound)
                    m_Socket.Close();
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