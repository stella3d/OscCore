using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace OscCore.Tests
{
    public class SimpleListPerformanceTests
    {
        const int k_Count = 1000;

        static readonly Stopwatch Stopwatch = new Stopwatch();
        
        int[] m_TestData;

        int[] m_Array;
        readonly List<int> m_List = new List<int>(k_Count);
        readonly Buffer<int> m_Buffer = new Buffer<int>(k_Count);

        long m_ArraySetTicks;
        long m_ListAddTicks;
        long m_BufferSetTicks;
        long m_SimpleListAddTicks;
        long m_SimpleListIndexSetTicks;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_TestData = new int[k_Count];
            m_Array = new int[k_Count];
            for (int i = 0; i < m_TestData.Length; i++)
                m_TestData[i] = Random.Range(-255, 255);
        }

        [Test]
        public void Add()
        {
            Stopwatch.Restart();
            for (int i = 0; i < m_TestData.Length; i++)
            {
                m_Array[i] = m_TestData[i];
            }
            Stopwatch.Stop();
            m_ArraySetTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < m_TestData.Length; i++)
            {
                m_List.Add(m_TestData[i]);
            }
            Stopwatch.Stop();
            m_ListAddTicks = Stopwatch.ElapsedTicks;
            Stopwatch.Restart();
            
            var arr = m_Buffer.Array;
            var bi = 0;
            for (; bi < m_TestData.Length; bi++)
                arr[bi] = m_TestData[bi];

            m_Buffer.Count = bi;
            
            Stopwatch.Stop();
            m_SimpleListIndexSetTicks = Stopwatch.ElapsedTicks;

            Debug.Log($"Add times - array: {m_ArraySetTicks}\nlist: {m_ListAddTicks}, " +
                      $"buffer manual array with count {m_SimpleListIndexSetTicks}");
        }



        [Test]
        public unsafe void ReadFloatMethods()
        {
            var bytes = TestUtil.RandomFloatBytes(4096);
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = BitConverter.ToSingle(bytes, i);
            }
            Stopwatch.Stop();
            var bitConverterTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = OscValueHandle.ReadFloat32Unsafe(bytes, i);
            }
            Stopwatch.Stop();
            var unsafeConvertTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                float f;
                fixed (byte* ptr = &bytes[i])
                {
                    f = *ptr;
                }
            }
            Stopwatch.Stop();
            var unsafeConvertInlineTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    float f = * (ptr + i);
                }
            }

            Stopwatch.Stop();
            var uInlineTicks1 = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                
                var fPtr = (float*) ptr;
                var end = bytes.Length / 4;
                for (int i = 0; i < end; i++)
                {
                     float f =  *(fPtr + i);
                }
            }

            Stopwatch.Stop();
            var uInlineTicks2 = Stopwatch.ElapsedTicks;
            
            Debug.Log($"float read times - bit converter: {bitConverterTicks}, unsafe: {unsafeConvertTicks} " +
                      $"inline unsafe {unsafeConvertInlineTicks}, single-fix {uInlineTicks1}, with fptr conversion {uInlineTicks2}");
        }
        
        [Test]
        public unsafe void ReadIntMethods()
        {
            const int count = 4096;
            var bytes = TestUtil.RandomIntBytes(count);

            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = BitConverter.ToInt32(bytes, i);
            }
            Stopwatch.Stop();
            var bitConverterTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = OscValueHandle.ReadInt32Unsafe(bytes, i);
            }
            Stopwatch.Stop();
            var unsafeConvertTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                int f;
                fixed (byte* ptr = &bytes[i])
                {
                    f = *ptr;
                }
            }
            Stopwatch.Stop();
            var unsafeConvertInlineTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    int f = *(ptr + i);
                }
            }

            Stopwatch.Stop();
            var uInlineTicks2 = Stopwatch.ElapsedTicks;
            
            Debug.Log($"int read times - bit converter: {bitConverterTicks}, unsafe: {unsafeConvertTicks}, " +
                      $"inline unsafe {unsafeConvertInlineTicks}, with ptr math {uInlineTicks2}");
        }

        [Test]
        // this should apply the same to MIDI messages - unsafe inline seems fastest
        public unsafe void Color32Parsing()
        {
            const int count = 4096;
            var bytes = TestUtil.RandomColor32Bytes(count);

            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = OscValueHandle.ReadColor32(bytes, i);
            }
            Stopwatch.Stop();
            var sTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var r = bytes[i];
                var g = bytes[i + 1];
                var b = bytes[i + 2];
                var a = bytes[i + 3];
                var f = new Color32(r, g, b, a);
            }
            Stopwatch.Stop();
            var sInlineTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = OscValueHandle.ReadColor32Unsafe(bytes, i);
            }
            Stopwatch.Stop();
            var uTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += 4)
            {
                fixed (byte* ptr = &bytes[i])
                {
                    var f = *(Color32*) ptr;
                }
            }
            Stopwatch.Stop();
            var uInlineTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    var f = *(Color32*) (ptr + i);
                }
            }

            Stopwatch.Stop();
            var uInline2Ticks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                for (int i = 0; i < bytes.Length; i += 4)
                {
                    var f = OscParser.ReadPointer<Color32>(ptr, i);
                }
            }

            Stopwatch.Stop();
            var ptrReadTicks = Stopwatch.ElapsedTicks;

            Debug.Log($"element count {count / 4} - safe {sTicks}, inline {sInlineTicks}, ptr read method {ptrReadTicks}\n" + 
                      $"unsafe {uTicks}, inline {uInlineTicks}, inline with ptr increment {uInline2Ticks}");
        }
        
        [Test]
        public unsafe void TimestampParsing()
        {
            const int count = 4096;
            var bytes = TestUtil.RandomTimestampBytes(count);

            var size = sizeof(NtpTimestamp);
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += size)
            {
                var f = OscValueHandle.ReadTimestamp(bytes, i);
            }
            Stopwatch.Stop();
            var sTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += size)
            {
                var f = OscValueHandle.ReadTimestampUnsafe(bytes, i);
            }
            Stopwatch.Stop();
            var uTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            for (int i = 0; i < bytes.Length; i += size)
            {
                fixed (byte* ptr = &bytes[i])
                {
                    var f = *(NtpTimestamp*) ptr;
                }
            }
            Stopwatch.Stop();
            var uInlineTicks = Stopwatch.ElapsedTicks;
            
            Stopwatch.Restart();
            fixed (byte* ptr = bytes)
            {
                for (int i = 0; i < bytes.Length; i += size)
                {
                    var f = *(NtpTimestamp*) (ptr + i);
                }
            }

            Stopwatch.Stop();
            var uInline2Ticks = Stopwatch.ElapsedTicks;

            Debug.Log($"timestamp parsing - element count {count / 4}, ticks - safe {sTicks}, unsafe {uTicks},\n" + 
                      $"inline {uInlineTicks}, inline with ptr increment {uInline2Ticks}");
        }
    }
}
