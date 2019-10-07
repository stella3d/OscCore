using System;
using System.Diagnostics;
using NUnit.Framework;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace OscCore.Tests
{
    public class MessageReadPerformanceTests
    {
        const int k_Count = 4096;

        static readonly Stopwatch Stopwatch = new Stopwatch();
        
        int[] m_IntSourceData;
        float[] m_FloatSourceData;
        
        byte[] m_BigEndianIntSourceBytes;
        byte[] m_BigEndianFloatSourceBytes;
        
        int[] m_IntReadData;
        float[] m_FloatReadData;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_IntSourceData = new int[k_Count];
            m_FloatSourceData = new float[k_Count];
            m_IntReadData = new int[k_Count];
            m_BigEndianIntSourceBytes = new byte[k_Count * 4];
            m_BigEndianFloatSourceBytes = new byte[k_Count * 4];

            for (int i = 0; i < m_IntSourceData.Length; i++)
                m_IntSourceData[i] = Random.Range(-10000, 10000);
            
            for (int i = 0; i < m_FloatSourceData.Length; i++)
                m_FloatSourceData[i] = Random.Range(-100f, 100f);
        }

        [SetUp]
        public void BeforeEach()
        {
            for (int i = 0; i < m_IntSourceData.Length; i++)
            {
                var lBytes = BitConverter.GetBytes(m_IntSourceData[i]);
                var bBytes = TestUtil.ReversedCopy(lBytes);

                var elementStart = i * 4;
                for (int j = 0; j < bBytes.Length; j++)
                    m_BigEndianIntSourceBytes[elementStart + j] = bBytes[j];
            }
            
            for (int i = 0; i < m_FloatSourceData.Length; i++)
            {
                var lBytes = BitConverter.GetBytes(m_FloatSourceData[i]);
                var bBytes = TestUtil.ReversedCopy(lBytes);

                var elementStart = i * 4;
                for (int j = 0; j < bBytes.Length; j++)
                    m_BigEndianFloatSourceBytes[elementStart + j] = bBytes[j];
            }
        }

        [Test]
        public void InlineVsFunction_Int()
        {
            Stopwatch.Restart();
            for (int i = 0; i < m_BigEndianIntSourceBytes.Length; i += 4)
            {
                var readInt = OscParser.ReadBigEndianInt(m_BigEndianIntSourceBytes, i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{k_Count} elements, safe int32 read from big-endian : {Stopwatch.ElapsedTicks} ticks");
            
            Stopwatch.Restart();
            for (int i = 0; i < m_BigEndianIntSourceBytes.Length; i += 4)
            {
                var readInt = m_BigEndianIntSourceBytes[i    ] << 24 |
                              m_BigEndianIntSourceBytes[i + 1] << 16 |
                              m_BigEndianIntSourceBytes[i + 2] <<  8 |
                              m_BigEndianIntSourceBytes[i + 3];
            }
            Stopwatch.Stop();
            
            Debug.Log($"{k_Count} elements, inline safe int32 read from big-endian : {Stopwatch.ElapsedTicks} ticks");
        }

        static OscMessageValues FromBytes(byte[] bytes, int count, TypeTag tag, int byteSize = 4)
        {
            var values = new OscMessageValues(bytes, count);
            for (int i = 0; i < count; i++)
            {
                values.Offsets[i] = i * byteSize;
                values.Tags[i] = tag;
            }

            values.ElementCount = count;
            return values;
        }

        [Test]
        public void ReadFloatElement_CheckedVsUnchecked()
        {
            const int count = 1024;
            var values = FromBytes(m_BigEndianFloatSourceBytes, count, TypeTag.Float32);

            float value = 0f;
            Stopwatch.Restart();
            for (int i = 0; i < count; i += 4)
            {
                value = values.ReadFloatElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count} elements, checked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");

            Stopwatch.Restart();
            for (int i = 0; i < count; i += 4)
            {
                value = values.ReadFloatElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count} elements, unchecked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");
        }
        
        [Test]
        public void ReadIntElement_CheckedVsUnchecked()
        {
            const int count = 1024;
            var values = FromBytes(m_BigEndianIntSourceBytes, count, TypeTag.Int32);

            float value = 0f;
            Stopwatch.Restart();
            for (int i = 0; i < count; i += 4)
            {
                value = values.ReadIntElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count} elements, checked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");

            Stopwatch.Restart();
            for (int i = 0; i < count; i += 4)
            {
                value = values.ReadIntElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count} elements, unchecked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");
        }
    }
}
