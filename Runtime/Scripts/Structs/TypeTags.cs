using System;
using System.Text;

namespace OscCore
{
    public struct TypeTags : IEquatable<TypeTags>
    {
        readonly byte[] m_Data;
        
        public TypeTags(string address)
        {
            if (!IsValid(address, out var err))
                throw new ArgumentException(err, nameof(address));

            m_Data = new byte[address.Get4ByteChunkLength()];
            address.CopyToASCII(m_Data);
        }

        static readonly string k_BeginCommaError = "Type tags must start with \",\"";
        
        internal static bool IsValid(string tags, out string err)
        {
            if (!StringUtil.IsNonEmptyASCII(tags, out err))
                return false;

            if (tags[0] != ',') 
            {
                err = k_BeginCommaError; 
                return false;
            }

            err = null;
            return true;
        }
        
        public override string ToString() => Encoding.ASCII.GetString(m_Data);

        public bool Equals(TypeTags other)
        {
            if (other.m_Data.Length != m_Data.Length)
                return false;

            // start at 1 because 0 is always ','
            for (var i = 1; i < m_Data.Length; i++)
                if (m_Data[i] != other.m_Data[i])
                    return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeTags other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (m_Data[m_Data.Length - 1] ^ m_Data.Length) * 397;
            }
        }

        public static bool operator ==(TypeTags left, TypeTags right) => left.Equals(right);
        public static bool operator !=(TypeTags left, TypeTags right) => !left.Equals(right);
    }
}