using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace BlobHandles.Tests
{
    public class BlobHandleTests
    {
        [Test]
        public unsafe void BlobHandle_PointerConstructor()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(16, 16));
            fixed (byte* bPtr = b1)
            {
                var handle = new BlobHandle(bPtr, b1.Length);
                Assert.True(handle.Pointer == bPtr);
                Assert.True(handle.Length == b1.Length);
            }
        }
        
        [Test]
        public unsafe void BlobHandle_IntPointerConstructor()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(16, 16));
            fixed (byte* bPtr = b1)
            {
                var handle = new BlobHandle((IntPtr) bPtr, b1.Length);
                Assert.True(handle.Pointer == bPtr);
                Assert.True(handle.Length == b1.Length);
            }
        }
        
        [Test]
        public unsafe void BlobHandle_DifferentAddressesStillEqual()
        {
            var str = TestData.RandomString(32, 32);
            var b1 = Encoding.ASCII.GetBytes(str);
            var b2 = Encoding.ASCII.GetBytes(str);

            BlobHandle h1, h2;
            fixed (byte* bPtr = b1) h1 = new BlobHandle(bPtr, b1.Length);
            fixed (byte* bPtr = b2) h2 = new BlobHandle(bPtr, b2.Length);
            
            Assert.AreEqual(h1.GetHashCode(), h2.GetHashCode());
            Assert.AreEqual(h1, h2);
        }
        
        [TestCase(TestData.StringConstants.EatTheRich)]
        [TestCase(TestData.StringConstants.M4A)]
        [TestCase(TestData.StringConstants.HealthJustice)]
        public void BlobString_ToString_OutputIsIdentical(string input)
        {
            var blobString = new BlobString(input);
            Debug.Log($"input - {input}, managed int string output - {blobString}");
            Assert.AreEqual(input, blobString.ToString());
            blobString.Dispose();
        }
        
        [TestCase(TestData.StringConstants.EatTheRich)]
        [TestCase(TestData.StringConstants.M4A)]
        [TestCase(TestData.StringConstants.HealthJustice)]
        public void BlobString_GetHashCode_OutputSameAcrossInstances(string input)
        {
            var blobString1 = new BlobString(input);
            var blobString2 = new BlobString(input);
            
            var hashCode1 = blobString1.GetHashCode();
            var hashCode2 = blobString2.GetHashCode();
            blobString1.Dispose();
            blobString2.Dispose();
            
            Assert.AreEqual(hashCode1, hashCode2);
        }
    }
}