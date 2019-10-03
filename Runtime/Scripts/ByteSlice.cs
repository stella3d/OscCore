using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OscCore
{
    public class ByteSlice
    {
        public byte[] Bytes;
        public int Start;
        public int End;

        public ByteSlice(byte[] bytes)
        {
            Bytes = bytes;
        }

        public void SetBounds(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}

