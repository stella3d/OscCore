using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;

namespace OscCore.Tests
{
    public class ParsingTests
    {
        const int k_BufferSize = 4096;
        readonly byte[] m_Buffer = new byte[k_BufferSize];
        GCHandle m_BufferHandle;
        OscParser m_Parser;

        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_BufferHandle = GCHandle.Alloc(m_Buffer, GCHandleType.Pinned);
            m_Parser = new OscParser(m_Buffer);
        }

        [SetUp]
        public void BeforeEach()
        {
            m_Parser.MessageValues.ElementCount = 0;
        }

        [OneTimeTearDown]
        public void AfterAll()
        { 
            if(m_BufferHandle.IsAllocated) m_BufferHandle.Free();
        }

        [TestCaseSource(typeof(TagsTestData), nameof(TagsTestData.StandardTagParseCases))]
        public void SimpleTagParsing(TypeTagParseTestCase test)
        {
            var tagSize = m_Parser.ParseTags(test.Bytes, test.Start);
            var tagCount = tagSize - 1; // remove ','
            
            Assert.AreEqual(test.Expected.Length, tagCount);
            var tags = m_Parser.MessageValues.Tags;
            for (var i = 0; i < tagCount; i++)
            {
                var tag = tags[i];
                Debug.Log(tag);
                Assert.AreEqual(test.Expected[i], tag);
            }
        }

        [Test]
        public void TagParsing_MustStartWithComma()
        {
            var commaAfterStart = new byte[] { 0, (byte) ',', 1, 2 };
            Assert.Zero(m_Parser.ParseTags(commaAfterStart));
            Assert.Zero(m_Parser.MessageValues.ElementCount);

            var noCommaBeforeTags = new byte[] { (byte)'f', (byte)'i', 1, 2 };
            Assert.Zero(m_Parser.ParseTags(noCommaBeforeTags));            
            Assert.Zero(m_Parser.MessageValues.ElementCount);
        }

        [TestCaseSource(typeof(MidiTestData), nameof(MidiTestData.Basic))]
        public void BasicMidiParsing(byte[] bytes, int offset, byte[] expected)
        {
            var midi = new MidiMessage(bytes, offset);
            Debug.Log(midi);
            Assert.AreEqual(expected[0], midi.PortId);
            Assert.AreEqual(expected[1], midi.Status);
            Assert.AreEqual(expected[2], midi.Data1);
            Assert.AreEqual(expected[3], midi.Data2);
        }
    }
}
