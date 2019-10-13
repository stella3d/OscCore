using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace OscCore
{
    public sealed class OscServer : IDisposable
    {
        readonly Socket m_Socket;
        readonly Thread m_Thread;
        bool m_Disposed;
        readonly int m_BufferSize;
        GCHandle m_BufferHandle;

        readonly List<MonitorCallback> m_MonitorCallbacks = new List<MonitorCallback>();
        
        public int Port { get; private set; }
        public OscAddressSpace AddressSpace { get; private set; }
        
        public OscServer(int port, int bufferSize = 4096)
        {
            AddressSpace = new OscAddressSpace();
            m_BufferSize = bufferSize;
            m_MonitorCallbacks.Clear();
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { ReceiveTimeout = 69 };
            m_Thread = new Thread(Serve);
            Port = port;
        }

        public void Start()
        {
            m_Socket.Bind(new IPEndPoint(IPAddress.Any, Port));
            m_Thread.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            var buffer = new byte[m_BufferSize];
            m_BufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var bufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();

            var socket = m_Socket;
            var parser = new OscParser(buffer, m_BufferHandle);
            var addressToMethod = AddressSpace.AddressToMethod;

            string tempAddress = null;
            while (!m_Disposed)
            {
                try
                {
                    int receivedByteCount = socket.Receive(buffer);
                    if (receivedByteCount <= 0) continue;
                    
                    var addressLength = parser.FindAddressLength();
                    if (addressLength < 0) continue;                         // address didn't start with '/'

                    var tagCount = parser.ParseTags(buffer, addressLength);
                    if (tagCount <= 0) continue;
                    
                    // skip the ',' and align to 4 bytes
                    var offset = addressLength + (tagCount + 4) & ~3;
                    parser.FindOffsets(offset);

                    if (addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var method))
                    {
                        // call the method(s) associated with this OSC address    
                        method.Invoke(parser.MessageValues);
                        // if we have any monitors, we need a string address
                        if (m_MonitorCallbacks.Count > 0)
                            tempAddress = Encoding.UTF8.GetString(bufferPtr, addressLength);
                    }
                    else
                    {
                        // if we didn't find an address match, compare the string against all of our known patterns  
                        tempAddress = Encoding.UTF8.GetString(bufferPtr, addressLength);
                        if (AddressSpace.TryMatchPatternHandler(tempAddress, out method))
                            method.Invoke(parser.MessageValues);  
                    }

                    // call all monitor callbacks
                    foreach (var callback in m_MonitorCallbacks)
                        callback(tempAddress, parser.MessageValues);
                }
                catch (SocketException) {}
                catch (ThreadAbortException) {}
                catch (Exception e)
                {
                    if (!m_Disposed) UnityEngine.Debug.LogException(e); 
                    break;
                }
            }
        }

        void Dispose(bool disposing)
        {
            if (m_Disposed) return;
            m_Disposed = true;

            if(m_BufferHandle.IsAllocated) m_BufferHandle.Free();
            if (disposing)
            {
                m_Socket?.Close();
                m_Thread?.Join();
                AddressSpace = null;
            }
        }

        ~OscServer()
        {
            Dispose(false);
        }
    }
}