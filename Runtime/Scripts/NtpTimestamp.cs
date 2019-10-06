using System;
using System.Runtime.CompilerServices;

namespace OscCore
{
    public struct NtpTimestamp
    {
        public readonly uint Seconds;
        public readonly uint Fractions;

        public NtpTimestamp(uint seconds, uint fractions)
        {
            Seconds = seconds;
            Fractions = fractions;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NtpTimestamp FromBytesUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            { 
                var ts = *(NtpTimestamp*) ptr;
                return new NtpTimestamp(ts.Seconds, ReverseBytes(ts.Fractions));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NtpTimestamp FromBytes(byte[] bytes, int offset)
        {
            var seconds = BitConverter.ToUInt32(bytes, offset);
            var fractions = ReverseBytes(BitConverter.ToUInt32(bytes, offset + 4));
            return new NtpTimestamp(seconds, fractions);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}