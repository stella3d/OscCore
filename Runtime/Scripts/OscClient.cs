using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace OscCore
{
    public class OscClient
    {
        readonly Socket m_Socket;
        
        public IPEndPoint Destination { get; }

        readonly OscWriter m_Writer;

        public OscWriter Writer => m_Writer;

        public OscClient(string ipAddress, int port)
        {
            m_Writer = new OscWriter();
            
            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (ipAddress == "255.255.255.255")
                m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            
            Destination = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            m_Socket.Connect(Destination);
        }
        
        /// <summary>Send a message with no elements</summary>
        public void Send(string address)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.Write(",");
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        static readonly uint k_Int32TypeTagBytes = GetAlignedAsciiBytes(",i")[0];
        
        /// <summary>Send a message with a single 32-bit integer element</summary>
        public void Send(string address, int element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_Int32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with 2 32-bit integer elements</summary>
        public void Send(string address, int element1, int element2)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",ii";
            m_Writer.Write(typeTags);
            m_Writer.Write(element1);
            m_Writer.Write(element2);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with 3 32-bit integer elements</summary>
        public void Send(string address, int element1, int element2, int element3)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",iii";
            m_Writer.Write(typeTags);
            m_Writer.Write(element1);
            m_Writer.Write(element2);
            m_Writer.Write(element3);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static readonly uint k_Float32TypeTagBytes = GetAlignedAsciiBytes(",f")[0];

        /// <summary>Send a message with a single 32-bit float element</summary>
        public void Send(string address, float element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_Float32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with 2 32-bit float elements</summary>
        public void Send(string address, float element1, float element2)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",ff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element1);
            m_Writer.Write(element2);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with 3 32-bit float elements</summary>
        public void Send(string address, float element1, float element2, float element3)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",fff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element1);
            m_Writer.Write(element2);
            m_Writer.Write(element3);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        static readonly uint k_StringTypeTagBytes = GetAlignedAsciiBytes(",s")[0];
        
        /// <summary>Send a message with a single string element</summary>
        public void Send(string address, string element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_StringTypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static readonly uint k_BlobTypeTagBytes = GetAlignedAsciiBytes(",b")[0];

        /// <summary>Send a message with a single blob element</summary>
        /// <param name="address">The OSC address</param>
        /// <param name="bytes">The bytes to copy from</param>
        /// <param name="length">The number of bytes in the blob element</param>
        /// <param name="start">The index in the bytes array to start copying from</param>
        public void Send(string address, byte[] bytes, int length, int start = 0)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_BlobTypeTagBytes);
            m_Writer.Write(bytes, length, start);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with a 2 32-bit float elements</summary>
        public void Send(string address, Vector2 element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",ff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with a 3 32-bit float elements</summary>
        public void Send(string address, Vector3 element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",fff";
            m_Writer.Write(typeTags);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static readonly uint k_Int64TypeTagBytes = GetAlignedAsciiBytes(",d")[0];
        
        /// <summary>Send a message with a single 64-bit float element</summary>
        public void Send(string address, double element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_Int64TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        static readonly uint k_Float64TypeTagBytes = GetAlignedAsciiBytes(",h")[0];

        /// <summary>Send a message with a single 64-bit integer element</summary>
        public void Send(string address, long element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_Float64TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static readonly uint k_Color32TypeTagBytes = GetAlignedAsciiBytes(",r")[0];
        
        /// <summary>Send a message with a single 32-bit color element</summary>
        public void Send(string address, Color32 element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_Color32TypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static readonly uint k_MidiTypeTagBytes = GetAlignedAsciiBytes(",m")[0];
        
        /// <summary>Send a message with a single MIDI message element</summary>
        public void Send(string address, MidiMessage element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.WriteTagBytes(k_MidiTypeTagBytes);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with a single ascii character element</summary>
        public void Send(string address, char element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string typeTags = ",c";
            m_Writer.Write(typeTags);
            m_Writer.Write(element);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }

        /// <summary>Send a message with a single True or False tag element</summary>
        public void Send(string address, bool element)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            const string trueTag = ",T";
            const string falseTag = ",F";
            m_Writer.Write(element ? trueTag : falseTag);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        /// <summary>Send a message with a single Nil ('N') tag element</summary>
        public void SendNil(string address)
        {
            const string tag = ",N";
            SendNull(address, tag);
        }
        
        /// <summary>Send a message with a single Infinitum ('I') tag element</summary>
        public void SendInfinitum(string address)
        {
            const string tag = ",I";
            SendNull(address, tag);
        }

        void SendNull(string address, string tags)
        {
            m_Writer.Reset();
            m_Writer.Write(address);
            m_Writer.Write(tags);
            m_Socket.Send(m_Writer.Buffer, m_Writer.Length, SocketFlags.None);
        }
        
        static unsafe uint[] GetAlignedAsciiBytes(string input)
        {
            var count = Encoding.ASCII.GetByteCount(input);
            var alignedCount = (count + 3) & ~3;
            var bytes = new uint[alignedCount / 4];

            fixed (uint* bPtr = bytes)
            {
                fixed (char* strPtr = input)
                {
                    Encoding.ASCII.GetBytes(strPtr, input.Length, (byte*) bPtr, count);
                }
            }

            return bytes;
        }
    }
}