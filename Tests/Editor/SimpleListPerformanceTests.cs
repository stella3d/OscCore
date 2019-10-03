using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
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
        public unsafe void Add()
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
            
            Stopwatch.Restart();
            
            var ui = 0;
            for (; bi < m_TestData.Length; bi++)
                arr[bi] = m_TestData[bi];

            m_Buffer.Count = bi;
            
            Stopwatch.Stop();
            var unsafeTicks = Stopwatch.ElapsedTicks;

            Debug.Log($"Add times - array: {m_ArraySetTicks}\nlist: {m_ListAddTicks}, " +
                      $"buffer manual array with count {m_SimpleListIndexSetTicks}");
        }


        byte[] RandomFloatBytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = Random.Range(-1f, 1f);
                var fBytes = BitConverter.GetBytes(f);
                for (int j = 0; j < fBytes.Length; j++)
                {
                    bytes[i + j] = fBytes[j];
                }
            }

            return bytes;
        }
        
        byte[] RandomIntBytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var iValue = Random.Range(-1000, 1000);
                var iBytes = BitConverter.GetBytes(iValue);
                for (int j = 0; j < iBytes.Length; j++)
                    bytes[i + j] = iBytes[j];
            }

            return bytes;
        }


        [Test]
        public unsafe void ReadFloatMethods()
        {
            var bytes = RandomFloatBytes(4096);
            
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
                var f = OscParser.ReadFloat32Unsafe(bytes, i);
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
            
            Debug.Log($"float read times - bit converter: {bitConverterTicks}, unsafe: {unsafeConvertTicks} " +
                      $"inline unsafe {unsafeConvertInlineTicks}");
        }
        
        [Test]
        public unsafe void ReadIntMethods()
        {
            const int count = 4096;
            var bytes = RandomIntBytes(count);

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
                var f = OscParser.ReadInt32Unsafe(bytes, i);
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
            
            Debug.Log($"int read times - bit converter: {bitConverterTicks}, unsafe: {unsafeConvertTicks}, " +
                      $"inline unsafe {unsafeConvertInlineTicks}");
        }
    }
}
