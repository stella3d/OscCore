using System;
using NUnit.Framework;

namespace OscCore.Tests
{
    public class OscWriterTests
    {
        const string bpmAddress = "/composition/bpm";
        
        OscWriter m_Writer;
        
        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_Writer = new OscWriter();
        }
        
        [TestCase(130)]
        [TestCase(144)]
        public void WriteInt32(int value)
        {
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 4, m_Writer.Length);
            // this tests both that it wrote to the right place in the buffer as well as that the value is right
            var convertedBack = BitConverter.ToInt32(m_Writer.Buffer, lengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(0.00001f)]
        [TestCase(0.887924529f)]
        [TestCase(144f)]
        public void WriteFloat32(float value)
        {
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 4, m_Writer.Length);
            // this tests both that it wrote to the right place in the buffer as well as that the value is right
            var convertedBack = BitConverter.ToSingle(m_Writer.Buffer, lengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
    }
}