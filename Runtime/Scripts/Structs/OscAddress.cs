using System;
using System.Text;

namespace OscCore
{
    public struct OscAddress : IEquatable<OscAddress>
    {
        readonly uint[] m_Data;
        readonly int m_UnalignedByteCount;
        
        public OscAddress(string address)
        {
            if (!IsValid(address, out var err))
                throw new ArgumentException(err, nameof(address));

            m_UnalignedByteCount = address.Length;
            m_Data = new uint[address.Get4ByteChunkLength()];
            address.CopyTo4AlignedASCII(m_Data);
        }

        static readonly string k_NullEmptyError = "Address cannot be null or empty";
        static readonly string k_OnlyASCIIError = "Address can only contain ASCII characters";
        static readonly string k_BeginSlashError = "Address must start with \"/\"";
        
        internal static bool IsValid(string addr, out string err)
        {
            if (string.IsNullOrEmpty(addr)) 
            {
                err = k_NullEmptyError; 
                return false;
            }
            if (!addr.IsASCII()) 
            {
                err = k_OnlyASCIIError; 
                return false;
            }
            if (addr[0] != '/') 
            {
                err = k_BeginSlashError; 
                return false;
            }

            err = null;
            return true;
        }

        
        public override unsafe string ToString()
        {
            fixed (uint* ptr = m_Data)
                return Encoding.ASCII.GetString((byte*) ptr, m_UnalignedByteCount);
        }

        public bool Equals(OscAddress other)
        {
            if (other.m_Data.Length != m_Data.Length)
                return false;

            for (var i = 0; i < m_Data.Length; i++)
                if (m_Data[i] != other.m_Data[i])
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is OscAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int) (m_Data[0] ^ m_UnalignedByteCount) * 397;
            }
        }

        public static bool operator ==(OscAddress left, OscAddress right) => left.Equals(right);
        public static bool operator !=(OscAddress left, OscAddress right) => !left.Equals(right);
    }
}