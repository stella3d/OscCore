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
}
