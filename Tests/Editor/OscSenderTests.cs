using NUnit.Framework;
using UnityEngine;

namespace OscCore.Tests
{
    public class OscSenderTests
    {
        GameObject m_Object;
        OscSender m_Sender;
        
        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_Object = new GameObject("OscSender Test Object", typeof(OscSender));
            m_Sender = m_Object.GetComponent<OscSender>();
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            Object.DestroyImmediate(m_Object);
        }

        [Test]
        public void SwitchPort()
        {
            const int newPort = 10100;
            Assert.AreNotEqual(m_Sender.Port, newPort);
            m_Sender.Port = newPort;
            Assert.AreEqual(m_Sender.Port, newPort);
        }
        
        [Test]
        public void SwitchIpAddress_ValidInput()
        {
            const string newAddress = "127.0.0.10";
            Assert.AreNotEqual(m_Sender.IpAddress, newAddress);
            m_Sender.IpAddress = newAddress;
            Assert.AreEqual(m_Sender.IpAddress, newAddress);
        }
        
        [Test]
        public void SwitchIpAddress_InvalidInput()
        {
            const string invalidAddress1 = "69000.420000.0.1";
            m_Sender.IpAddress = invalidAddress1;
            Assert.AreNotEqual(m_Sender.IpAddress, invalidAddress1);
        }
    }
}
