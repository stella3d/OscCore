using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OscCore
{
    public class OscParser
    {
        public static void ParseTags(byte[] bytes, int start, SimpleList<TypeTag> tags)
        {
            tags.Reset();
            var tagIndex = start + 1;         // skip the starting ','

            while (true)
            {
                var tag = (TypeTag) bytes[tagIndex];
                if (!tag.IsSupported()) break;
                tags.Add(tag);
                tagIndex++;
            }
        }
    }
}

