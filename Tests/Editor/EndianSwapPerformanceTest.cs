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
    public class EndianSwapPerformanceTests
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
        public void ByteSwap4()
        {
            
        }
    }
}
