using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OscCore
{
    public struct NtpTimestamp : IEquatable<NtpTimestamp>
    {
        static readonly DateTime k_Epoch1900 = DateTime.Parse("1900-01-01 00:00:00.000");
        static readonly DateTime k_Epoch2036 = DateTime.Parse("2036-02-07 06:28:15");

        public readonly uint Seconds;
        public readonly uint Fractions;

        public NtpTimestamp(uint seconds, uint fractions)
        {
            Seconds = seconds;
            Fractions = fractions;
        }

        public NtpTimestamp(DateTime dt)
        {
            var epoch = dt < k_Epoch2036 ? k_Epoch1900 : k_Epoch2036;
            Seconds = (uint)(dt - epoch).TotalSeconds;
            Fractions = (uint)(0xFFFFFFFF * ((double)dt.Millisecond / 1000));
        }

        public static unsafe NtpTimestamp FromBigEndianBytes(byte* bufferPtr, int offset)
        {
            var ptr = bufferPtr + offset;

            var bSeconds = *(uint*) ptr;
            // swap bytes from big to little endian 
            uint seconds = (bSeconds & 0x000000FFU) << 24 | (bSeconds & 0x0000FF00U) << 8 |
                            (bSeconds & 0x00FF0000U) >> 8 | (bSeconds & 0xFF000000U) >> 24;

            var bFractions = *(uint*) ptr + 4;
            uint fractions = (bFractions & 0x000000FFU) << 24 | (bFractions & 0x0000FF00U) << 8 |
                           (bFractions & 0x00FF0000U) >> 8 | (bFractions & 0xFF000000U) >> 24;
            
            return new NtpTimestamp(seconds, fractions);
        }
        
        public static unsafe NtpTimestamp FromBigEndianBytesNew(byte* bufferPtr, int offset)
        {
            var ptr = bufferPtr + offset;

            var bSeconds = *(uint*) ptr;
            // swap bytes from big to little endian 
            uint seconds = (bSeconds & 0x000000FFU) << 24 | (bSeconds & 0x0000FF00U) << 8 |
                           (bSeconds & 0x00FF0000U) >> 8 | (bSeconds & 0xFF000000U) >> 24;

            var bFractions = *(uint*) ptr + 4;
            uint fractions = (bFractions & 0x000000FFU) << 24 | (bFractions & 0x0000FF00U) << 8 |
                             (bFractions & 0x00FF0000U) >> 8 | (bFractions & 0xFF000000U) >> 24;
            
            return new NtpTimestamp(seconds, fractions);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NtpTimestamp FromBigEndianBytes(byte[] bytes, int offset)
        {
            var seconds = ReverseBytes(BitConverter.ToUInt32(bytes, offset));
            var fractions = ReverseBytes(BitConverter.ToUInt32(bytes, offset + 4));
            return new NtpTimestamp(seconds, fractions);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
        
        // according to the spec, resolution is about 200 picoseconds
        const double k_FractionMillisecondMultiplier = 200 / (double) 1000000000;
        
        public DateTime ToDateTime()
        {
            // account for the special "now" value
            if (Fractions == 1) return DateTime.Now;
            var epoch = DateTime.Now < k_Epoch2036 ? k_Epoch1900 : k_Epoch2036;

            var fractionMs = Fractions * k_FractionMillisecondMultiplier;
            var seconds = (double) Seconds * 1000 + fractionMs;
            return epoch.AddMilliseconds(seconds);
        }

        public bool Equals(NtpTimestamp other)
        {
            return Seconds == other.Seconds && Fractions == other.Fractions;
        }

        public override bool Equals(object obj)
        {
            return obj is NtpTimestamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Seconds * 397) ^ (int) Fractions;
            }
        }

        public static bool operator ==(NtpTimestamp left, NtpTimestamp right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NtpTimestamp left, NtpTimestamp right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Seconds: {Seconds} , Fractions {Fractions}";
        }
    }
}