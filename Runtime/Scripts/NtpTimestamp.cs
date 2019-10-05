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
            fixed (byte* ptr = &bytes[offset]) { return *(NtpTimestamp*) ptr; }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NtpTimestamp FromBytes(byte[] bytes, int offset)
        {
            var seconds = BitConverter.ToUInt32(bytes, offset);
            var fractions = BitConverter.ToUInt32(bytes, offset + 4);
            return new NtpTimestamp(seconds, fractions);
        }
    }
}