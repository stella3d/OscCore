using System;
using System.Text;
using System.Collections.Generic;

namespace OscCore
{
    static class StringUtil
    {
        internal static byte[] ToStringBuffer = new byte[512];
        
        public static int Get4ByteChunkLength(this string str)
        {
            var alignedByteLen = str.Length.Align4();
            return alignedByteLen / 4;
        }
        
        
        static readonly string k_NullEmptyError = "cannot be null or empty";
        static readonly string k_OnlyASCIIError = "can only contain ASCII characters";
        
        internal static bool IsNonEmptyASCII(string str, out string err)
        {
            if (string.IsNullOrEmpty(str)) 
            {
                err = k_NullEmptyError; 
                return false;
            }
            if (!str.IsASCII())
            {
                err = k_OnlyASCIIError;
                return false;
            }

            err = null;
            return true;
        }
    }
}