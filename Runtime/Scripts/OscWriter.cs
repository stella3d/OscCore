using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OscCore
{
    public sealed unsafe class OscWriter
    {
        int m_Length;
        readonly byte[] m_Buffer;
        byte* m_Ptr;

        GCHandle m_BufferHandle;
        
        readonly float[] m_FloatSwap = new float[1];
        readonly byte* m_FloatSwapPtr;
        readonly GCHandle m_FloatSwapHandle;
        
        readonly double[] m_DoubleSwap = new double[1];
        readonly byte* m_DoubleSwapPtr;
        readonly GCHandle m_DoubleSwapHandle;

        readonly Color32[] m_Color32Swap = new Color32[1];
        readonly byte* m_Color32SwapPtr;
        readonly GCHandle m_Color32SwapHandle;
        
        readonly Color32* SwapBufferColor32Ptr;
        readonly GCHandle m_Swap32Handle;
        // used to swap bytes for 64-bit numbers when reading
        readonly byte[] m_SwapBuffer64 = new byte[8];
        readonly double* SwapBuffer64Ptr;
        readonly GCHandle m_Swap64Handle;
        
        public OscWriter(int capacity = 4096)
        {
            m_Buffer = new byte[capacity];
            m_Ptr = PtrUtil.Pin<byte, byte>(m_Buffer, out m_BufferHandle);
            m_FloatSwapPtr = PtrUtil.Pin<float, byte>(m_FloatSwap, out m_FloatSwapHandle);
            m_DoubleSwapPtr = PtrUtil.Pin<double, byte>(m_DoubleSwap, out m_DoubleSwapHandle);
            m_Color32SwapPtr = PtrUtil.Pin<Color32, byte>(m_Color32Swap, out m_Color32SwapHandle);
        }

        /// <summary>Write a 32-bit integer element</summary>
        public void Write(int data)
        {
            m_Buffer[m_Length++] = (byte) (data >> 24);
            m_Buffer[m_Length++] = (byte) (data >> 16);
            m_Buffer[m_Length++] = (byte) (data >>  8);
            m_Buffer[m_Length++] = (byte) (data);
        }
        
        /// <summary>Write a 32-bit floating point element</summary>
        public void Write(float data)
        {
            m_FloatSwap[0] = data;
            m_Buffer[m_Length++] = m_FloatSwapPtr[3];
            m_Buffer[m_Length++] = m_FloatSwapPtr[2];
            m_Buffer[m_Length++] = m_FloatSwapPtr[1];
            m_Buffer[m_Length++] = m_FloatSwapPtr[0];
        }
        
        /// <summary>Write an ASCII string element</summary>
        public void Write(string data)
        {
            foreach (var chr in data)
                m_Buffer[m_Length++] = (byte) chr;

            var alignedLength = (data.Length + 3) & ~3;
            for (int i = data.Length; i < alignedLength; i++)
                m_Buffer[m_Length++] = 0;
        }
        
        /// <summary>Write a blob element</summary>
        /// <param name="bytes">The bytes to copy from</param>
        /// <param name="length">The number of bytes in the blob element</param>
        /// <param name="start">The index in the bytes array to start copying from</param>
        public void Write(byte[] bytes, int length, int start = 0)
        {
            fixed (byte* bPtr = &bytes[start])
            {
                if (start + length > bytes.Length) 
                    return;
                
                // write the size 
                m_Buffer[m_Length++] = (byte) (length >> 24);
                m_Buffer[m_Length++] = (byte) (length >> 16);
                m_Buffer[m_Length++] = (byte) (length >>  8);
                m_Buffer[m_Length++] = (byte) (length);
                
                Buffer.BlockCopy(bytes, start, m_Buffer, m_Length, length);
            }
        }
        
        /// <summary>Write a 64-bit integer element</summary>
        public void Write(long data)
        {
            m_Buffer[m_Length++] = (byte) (data >> 56);
            m_Buffer[m_Length++] = (byte) (data >> 48);
            m_Buffer[m_Length++] = (byte) (data >> 40);
            m_Buffer[m_Length++] = (byte) (data >> 32);
            m_Buffer[m_Length++] = (byte) (data >> 24);
            m_Buffer[m_Length++] = (byte) (data >> 16);
            m_Buffer[m_Length++] = (byte) (data >>  8);
            m_Buffer[m_Length++] = (byte) (data);
        }
        
        /// <summary>Write a 64-bit floating point element</summary>
        public void Write(double data)
        {
            m_DoubleSwap[0] = data;
            m_Buffer[m_Length++] = m_DoubleSwapPtr[7];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[6];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[5];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[4];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[3];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[2];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[1];
            m_Buffer[m_Length++] = m_DoubleSwapPtr[0];
        }
        
        /// <summary>Write a 32-bit RGBA color element</summary>
        public void Write(Color32 data)
        {
            m_Color32Swap[0] = data;
            m_Buffer[m_Length++] = m_Color32SwapPtr[3];
            m_Buffer[m_Length++] = m_Color32SwapPtr[2];
            m_Buffer[m_Length++] = m_Color32SwapPtr[1];
            m_Buffer[m_Length++] = m_Color32SwapPtr[0];
        }
        
        /// <summary>Write a MIDI message element</summary>
        public void WriteMidi(MidiMessage data)
        {
            m_Buffer[m_Length++] = data.PortId;
            m_Buffer[m_Length++] = data.Status;
            m_Buffer[m_Length++] = data.Data1;
            m_Buffer[m_Length++] = data.Data2;
        }

        /// <summary>Write a single ascii character element</summary>
        public void WriteAsciiChar(char data)
        {
            // char is written in the last byte of the 4-byte block;
            m_Buffer[m_Length + 3] = (byte) data;
            m_Length++;
        }
    }
}