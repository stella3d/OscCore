using System.Collections;
using System.Collections.Generic;
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

        [TestCaseSource(typeof(TagsTestData), nameof(TagsTestData.TagParseCases))]
        public void ParsingTestsSimplePasses(TypeTagParseTestCase test)
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

        [Test]
        public void Color32Unsafe()
        {
            var cBytes = new byte[] { 50, 100, 200, 255 };

            var safeRead = OscParser.ReadColor32(cBytes, 0);
            var unSafeRead = OscParser.ReadColor32Unsafe(cBytes, 0);
            
            Debug.Log($"safe: {safeRead} , unsafe: {unSafeRead}");
            Assert.AreEqual(safeRead, unSafeRead);
        }
    }

    public class TypeTagParseTestCase
    {
        public readonly byte[] Bytes;
        public readonly int Start;
        public readonly TypeTag[] Expected;

        public TypeTagParseTestCase(byte[] bytes, int start, TypeTag[] expected)
        {
            Bytes = bytes;
            Start = start;
            Expected = expected;
        }
    }

    internal static class TagsTestData
    {
        public static IEnumerable TagParseCases 
        {
            get
            {
                var expected1 = new[] { TypeTag.Float32, TypeTag.Float32, TypeTag.Int32, TypeTag.String };
                var bytes1 = new[]
                {
                    (byte) ',', (byte) TypeTag.Float32, (byte) TypeTag.Float32, (byte) TypeTag.Int32,
                    (byte) TypeTag.String, (byte) 0, (byte) 0, (byte) 0
                };
                yield return new TypeTagParseTestCase(bytes1, 0, expected1);
            }
        }
    }
    
    internal static class MidiTestData
    {
        public static IEnumerable Basic 
        {
            get
            {
                var expected1 = new[]
                {
                    (byte)1,                     // port id
                    (byte)144,                   // status - ch1 note on
                    (byte) 60,                   // note # - 60 = middle c
                    (byte)100                    // note velocity
                };
                var bytes1 = new[]
                {
                    (byte) 0, (byte) 0, (byte) 1, (byte) 144, 
                    (byte) 60, (byte) 100, (byte) 0, (byte) 0, 
                };
                
                yield return new TestCaseData(bytes1, 2, expected1);
                
                var expected2 = new[]
                {
                    (byte) 16,                   // port id
                    (byte) 128,                  // status - ch1 note off
                    (byte) 72,                   // note C4
                    (byte) 42,                   // note velocity
                };
                var bytes2 = new[]
                {
                    (byte) 16, (byte) 128, (byte) 72, (byte) 42, (byte) 0, (byte) 0
                };
                
                yield return new TestCaseData(bytes2, 0, expected2);
            }
        }
    }
}
