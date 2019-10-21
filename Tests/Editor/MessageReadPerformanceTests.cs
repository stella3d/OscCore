using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MiniNtp;
using NUnit.Framework;
using UnityEngine;
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
        MidiMessage[] m_MidiSourceData;
        
        byte[] m_BigEndianIntSourceBytes;
        byte[] m_BigEndianFloatSourceBytes;
        byte[] m_MidiSourceBytes;
        byte[] m_TimeSourceBytes;

        List<GCHandle> m_Handles = new List<GCHandle>();
        
        int[] m_IntReadData;
        float[] m_FloatReadData;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_Handles.Clear();
            m_IntSourceData = new int[k_Count];
            m_FloatSourceData = new float[k_Count];
            m_IntReadData = new int[k_Count];
            m_BigEndianIntSourceBytes = new byte[k_Count * 4];
            m_BigEndianFloatSourceBytes = new byte[k_Count * 4];
            
            
            m_Handles.Add(GCHandle.Alloc(m_BigEndianIntSourceBytes, GCHandleType.Pinned));
            m_Handles.Add(GCHandle.Alloc(m_BigEndianFloatSourceBytes, GCHandleType.Pinned));

            m_MidiSourceBytes = TestUtil.RandomMidiBytes(k_Count * 4);
            m_TimeSourceBytes = TestUtil.RandomTimestampBytes(k_Count * 4);

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
            const int count = 2048;
            var values = FromBytes(m_BigEndianFloatSourceBytes, count, TypeTag.Float32);

            float value = 0f;
            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                value = values.ReadFloatElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count / 4} elements, checked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");

            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                value = values.ReadFloatElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count / 4} elements, unchecked float32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");
        }
        
        [Test]
        public void ReadIntElement_CheckedVsUnchecked()
        {
            const int count = 2048;
            var values = FromBytes(m_BigEndianIntSourceBytes, count, TypeTag.Int32);

            float value = 0f;
            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                value = values.ReadIntElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count / 4} elements, checked int32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");

            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                value = values.ReadIntElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count / 4} elements, unchecked int32 element read: {Stopwatch.ElapsedTicks} ticks, last value {value}");
        }
        
        [Test]
        public void ReadMidiMessageElement_CheckedVsUnchecked()
        {
            const int count = 2048;
            var values = FromBytes(m_MidiSourceBytes, count, TypeTag.MIDI);

            MidiMessage midi;
            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                midi = values.ReadMidiElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count / 4} elements, checked MIDI element read: {Stopwatch.ElapsedTicks} ticks");

            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                midi = values.ReadMidiElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count / 4} elements, unchecked MIDI element read: {Stopwatch.ElapsedTicks} ticks");
        }
        
        [Test]
        public void ReadColor32MessageElement_CheckedVsUnchecked()
        {
            const int count = 2048;
            var values = FromBytes(m_MidiSourceBytes, count, TypeTag.Color32);

            Color32 color32;
            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                color32 = values.ReadColor32Element(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count / 4} elements, checked Color32 element read: {Stopwatch.ElapsedTicks} ticks");

            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                color32 = values.ReadColor32ElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count / 4} elements, unchecked Color32 element read: {Stopwatch.ElapsedTicks} ticks");
        }
        
        [Test]
        public void ReadTimestampElement_CheckedVsUnchecked()
        {
            const int count = 2048;
            var values = FromBytes(m_TimeSourceBytes, count, TypeTag.TimeTag);

            NtpTimestamp ts;
            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                ts = values.ReadTimestampElement(i);
            }
            Stopwatch.Stop();

            Debug.Log($"{count / 8} elements, checked NTP timestamp element read: {Stopwatch.ElapsedTicks} ticks");

            Stopwatch.Restart();
            for (int i = 0; i < count; i++)
            {
                ts = values.ReadTimestampElementUnchecked(i);
            }
            Stopwatch.Stop();
            
            Debug.Log($"{count / 8} elements, unchecked NTP timestamp element read: {Stopwatch.ElapsedTicks} ticks");
        }
    }
}
