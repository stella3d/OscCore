using System;
using System.Net;
using NUnit.Framework;

namespace OscCore.Tests
{
    public class OscWriterTests
    {
        const string bpmAddress = "/composition/bpm";
        
        readonly OscWriter m_Writer = new OscWriter();

        [SetUp]
        public void BeforeEach()
        {
            m_Writer.Reset();
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
        
        [TestCase(50000000)]
        [TestCase(144 * 100000)]
        public void WriteInt64(long value)
        {
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 8, m_Writer.Length);
            var convertedBack = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(m_Writer.Buffer, lengthBefore));
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(0.00001f)]
        [TestCase(0.867924529f)]
        [TestCase(144f)]
        public void WriteFloat32(float value)
        {
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 4, m_Writer.Length);
            var convertedBack = BitConverter.ToSingle(m_Writer.Buffer, lengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [TestCase(0.00000001d)]
        [TestCase(0.8279245299754d)]
        [TestCase(144.1d * 1000d)]
        public void WriteFloat64(double value)
        {
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 8, m_Writer.Length);
            var convertedBack = BitConverter.ToDouble(m_Writer.Buffer, lengthBefore).ReverseBytes();
            Assert.AreEqual(value, convertedBack);
        }
        
        [Test]
        public void WriteMidi()
        {
            var value = new MidiMessage(1, 4, 16, 80);
            var lengthBefore = m_Writer.Length;
            m_Writer.Write(value);

            Assert.AreEqual(lengthBefore + 4, m_Writer.Length);
            var convertedBack = new MidiMessage(m_Writer.Buffer, lengthBefore);
            Assert.True(value == convertedBack);
        }
    }
}