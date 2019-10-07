using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using NUnit.Framework;
using UnityEngine;

namespace OscCore.Tests
{
    public class ParsingTests
    {
        //OscParser m_Parser = new OscParser();

        readonly Buffer<TypeTag> m_Tags = new Buffer<TypeTag>();

        [OneTimeSetUp]
        public void BeforeAll() { }

        [TestCaseSource(typeof(TagsTestData), nameof(TagsTestData.StandardTagParseCases))]
        public void SimpleTagParsing(TypeTagParseTestCase test)
        {
            OscParser.ParseTags(test.Bytes, test.Start, m_Tags);

            var arr = m_Tags.Array;
            for (var i = 0; i < m_Tags.Count; i++)
            {
                var tag = arr[i];
                Debug.Log(tag);
                Assert.AreEqual(test.Expected[i], tag);
            }
        }

        [TestCaseSource(typeof(MessageTestData), nameof(MessageTestData.Basic))]
        public void SimpleFloatMessageParsing(byte[] bytes, int length)
        {
            OscParser.Parse(bytes, length);
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


        [TestCase(0.01f)]
        [TestCase(1f)]
        [TestCase(14.4f)]
        public unsafe void EndiannessSwapFloat(float littleEndianValue)
        {
            var bBytes = ReversedCopy(BitConverter.GetBytes(littleEndianValue));
            fixed (byte* bfPtr = bBytes)
                OscParser.SwapX4(bfPtr);

            var read = BitConverter.ToSingle(bBytes, 0);
            Assert.AreEqual(littleEndianValue, read);
        }
        
        [TestCase(1)]
        [TestCase(69)]
        [TestCase(42000)]
        public unsafe void EndiannessSwapInt(int littleEndianValue)
        {
            var bBytes = TestUtil.ReversedCopy(BitConverter.GetBytes(littleEndianValue));
            fixed (byte* bfPtr = bBytes)
                OscParser.SwapX4(bfPtr);
            
            var read = BitConverter.ToInt32(bBytes, 0);
            Assert.AreEqual(littleEndianValue, read);
        }

        byte[] ReversedCopy(byte[] source)
        {
            var copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            Array.Reverse(copy);
            return copy;
        }

        [Test]
        public void ReadColor32_UnsafeMatchesSafe()
        {
            var cBytes = new byte[] { 50, 100, 200, 255 };
            var color32 = new Color32(cBytes[0], cBytes[1], cBytes[2], cBytes[3]);

            var safeRead = OscMessageValues.ReadColor32(cBytes, 0);
            var unSafeRead = OscMessageValues.ReadColor32Unsafe(cBytes, 0);
            
            Debug.Log($"constructor {color32}, safe: {safeRead} , unsafe: {unSafeRead}");
            Assert.AreEqual(color32, safeRead);
            Assert.AreEqual(color32, unSafeRead);
        }
        
        [Test]
        public void ReadMidi_UnsafeMatchesSafe()
        {
            var bytes = new byte[] { 1, 144, 60, 42 };
            var midiMessage = new MidiMessage(bytes[0], bytes[1], bytes[2], bytes[3]);

            var safeRead = OscMessageValues.ReadMidi(bytes, 0);
            var unSafeRead = OscMessageValues.ReadMidiUnsafe(bytes, 0);
            
            Debug.Log($"constructor {midiMessage}, safe: {safeRead} , unsafe: {unSafeRead}");
            Assert.AreEqual(midiMessage, safeRead);
            Assert.AreEqual(midiMessage, unSafeRead);
        }
    }
}
