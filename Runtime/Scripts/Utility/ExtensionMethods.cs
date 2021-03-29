using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OscCore
{
    static class ExtensionMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align4(this int self)
        {
            return (self + 3) & ~3;
        }

        internal static void SafeFree(this GCHandle handle)
        {
            if(handle.IsAllocated) handle.Free();
        }
        
        internal static int ClampPort(this int port)
        {
            if (port < 1024) port = 1024;
            if (port >= 65535) port = 65535;
            return port;
        }

        internal static bool IsASCII(this string str)
        {
            // single-byte ascii chars are valid utf-8, so if no char is >1 byte as UTF-8,
            // then we know it only contains ascii characters
            return System.Text.Encoding.UTF8.GetByteCount(str) == str.Length;
        }
        
        /// <summary>
        /// This should only ever be used on validated ASCII strings! No bounds checking is performed either.
        /// </summary>
        internal static void CopyToASCII(this string str, byte[] dest, int start = 0)
        {
            for (var i = 0; i < str.Length; i++)
                dest[start + i] = (byte) str[i];
        }

        static byte[] s_AsciiCopyBuffer = new byte[512];
        
        /// <summary>
        /// This should only ever be used on validated ASCII strings! No bounds checking is performed either.
        /// </summary>
        internal static void CopyTo4AlignedASCII(this string str, uint[] dest)
        {
            if (s_AsciiCopyBuffer.Length < str.Length)
                Array.Resize(ref s_AsciiCopyBuffer, str.Length.Align4() * 2);
                
            var byteLen = Encoding.ASCII.GetBytes(str, 0, str.Length, s_AsciiCopyBuffer, 0);
            Buffer.BlockCopy(s_AsciiCopyBuffer, 0, dest, 0, byteLen);
        }
    }
}