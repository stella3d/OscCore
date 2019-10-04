using System;
using UnityEngine;

namespace OscCore
{
    public class OscBlob
    {
        public int Size { get; private set; }

        // TODO - replace this with a buffer all blobs share ?
        public byte[] CopiedBuffer;

        public OscBlob(byte[] bytes, int offset)
        {
            Read(bytes, offset);
        }
        
        public unsafe OscBlob(byte[] bytes, int offset, int destOffset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                int size = *ptr;
                CopiedBuffer = new byte[Size + destOffset];
                fixed (byte* destPtr = &CopiedBuffer[0])
                {
                    Buffer.MemoryCopy(ptr + 4, destPtr + destOffset, size, size);
                }
            }
        }

        public void Read(byte[] bytes, int offset)
        {
            // TODO - ptr initializer ?
            Size = BitConverter.ToInt32(bytes, offset);
            var dataStart = offset + 4;
            var end = dataStart + Size;
            var alignedEnd = end.Align4();

            var alignedSize = alignedEnd - offset;
            if (CopiedBuffer == null)
                CopiedBuffer = new byte[alignedSize];
            else if (CopiedBuffer.Length <= alignedSize)
                Array.Resize(ref CopiedBuffer, Size * 2);

            Buffer.BlockCopy(bytes, dataStart, CopiedBuffer, 0, Size);
        }

        public override string ToString()
        {
            return $"OSC Blob - Byte Length: {Size}";
        }
    }
}

