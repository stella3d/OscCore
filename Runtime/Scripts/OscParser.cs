using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OscCore
{
    public class OscParser
    {
        public static void ParseTags(byte[] bytes, int start, Buffer<TypeTag> tags)
        {
            tags.Count = 0;
            var tagIndex = start + 1;         // skip the starting ','

            var outIndex = 0;
            var outArray = tags.Array;
            while (true)
            {
                var tag = (TypeTag) bytes[tagIndex];
                if (!tag.IsSupported()) break;
                outArray[outIndex] = tag;
                tagIndex++;
                outIndex++;
            }

            tags.Count = outIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ReadFloatUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadIntUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }
    }
}

