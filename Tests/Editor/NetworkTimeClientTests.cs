using NUnit.Framework;
using UnityEngine;

namespace OscCore.Tests
{
    public class NetworkTimeClientTests
    {
        readonly NetworkTimeClient m_Client = new NetworkTimeClient();
        
        [OneTimeSetUp]
        public void BeforeAll()
        {
            m_Client.Start();
        }
        
        [OneTimeTearDown]
        public void AfterAll()
        {
        }

        [Test]
        public void SendRequest()
        {
            var task = m_Client.SendTimeRequest();
            /*
            Debug.Log(task.Status);
            task.Wait();
            Debug.Log(task.Status);
            task.Wait();
            */
        }
    }
}