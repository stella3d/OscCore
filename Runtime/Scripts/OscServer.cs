using System;
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

        public int Port { get; private set; }
        public OscAddressSpace AddressSpace { get; private set; }
        
        public OscServer(int port, int bufferSize = 4096)
        {
            AddressSpace = new OscAddressSpace();
            m_BufferSize = bufferSize;
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

        unsafe void Serve()
        {
            var buffer = new byte[m_BufferSize];
            m_BufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var bufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();

            var socket = m_Socket;
            var parser = new OscParser(buffer, m_BufferHandle);
            var addressToMethod = AddressSpace.AddressToMethod;
            
            while (!m_Disposed)
            {
                try
                {
                    int receivedByteCount = socket.Receive(buffer);
                    if (receivedByteCount <= 0)
                        continue;
                    
                    var addressLength = parser.FindAddressLength();
                    
                    if (!addressToMethod.TryGetValueFromBytes(bufferPtr, addressLength, out var method))
                    {
                        // if we didn't find an address match, compare the string against all of our known patterns  
                        var addressStr = Encoding.UTF8.GetString(bufferPtr, addressLength);
                        // no pattern match means no way to handle this message
                        if(!AddressSpace.TryMatchPatternHandler(addressStr, out method))
                            continue;    
                    }

                    var tagCount = parser.ParseTags(buffer, addressLength);
                    if (tagCount > 0)
                    {
                        // skip the ',' and align to 4 bytes
                        var tagByteLength = ((tagCount + 1) + 3) & ~3;            
                        var offset = addressLength + tagByteLength;
                        
                        parser.FindOffsets(offset);
                        method.Invoke(parser.MessageValues);
                    }
                }
                catch (SocketException) {}
                catch (ThreadAbortException) {}
                catch (Exception e)
                {
                    if (!m_Disposed) UnityEngine.Debug.LogException(e); 
                    break;
                }
                
                //Thread.Sleep(5);
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