using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace BlobHandles.Tests
{
    public class BlobStringDictionaryTests
    {
        const int k_Value = 303;

        static readonly Dictionary<BlobHandle, int> k_Dictionary = new Dictionary<BlobHandle, int>();

        [SetUp]
        public void BeforeEach()
        {
            k_Dictionary.Clear();
        }
        
        [Test]
        public unsafe void Dictionary_TryGetValueFromBytes_Pointer()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(10, 50));
            var handle = new BlobHandle(b1);
            
            Assert.False(k_Dictionary.ContainsKey(handle));
            k_Dictionary.Add(handle, k_Value);
            
            fixed (byte* b1Ptr = b1)
            {
                Assert.True(k_Dictionary.TryGetValue(handle, out var value));
                Assert.True(k_Dictionary.TryGetValueFromBytes(b1Ptr, b1.Length, out var valueFromBytes));
                Assert.AreEqual(k_Value, value);    
                Assert.AreEqual(k_Value, valueFromBytes); 
            }
        }

        [Test]
        public void Dictionary_TryGetValueFromBytes_Array()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(10, 50));
            var handle = new BlobHandle(b1);
            
            Assert.False(k_Dictionary.ContainsKey(handle));
            k_Dictionary.Add(handle, k_Value);
            
            Assert.True(k_Dictionary.TryGetValue(handle, out var value));
            Assert.True(k_Dictionary.TryGetValueFromBytes(b1, out var valueFromBytes));
            Assert.AreEqual(k_Value, value);    
            Assert.AreEqual(k_Value, valueFromBytes);    
        }
        
        [Test]
        public void Dictionary_TryGetValueFromBytes_ArrayWithLength()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(30, 50));
            var length = b1.Length - 10;
            var handle = new BlobHandle(b1, length);
            
            Assert.False(k_Dictionary.ContainsKey(handle));
            k_Dictionary.Add(handle, k_Value);
            
            Assert.True(k_Dictionary.TryGetValue(handle, out var value));
            Assert.True(k_Dictionary.TryGetValueFromBytes(b1, length, out var valueFromBytes));
            Assert.AreEqual(k_Value, value);    
            Assert.AreEqual(k_Value, valueFromBytes);    
        }
        
        [Test]
        public void Dictionary_TryGetValueFromBytes_ArrayWithLengthAndOffset()
        {
            var b1 = Encoding.ASCII.GetBytes(TestData.RandomString(40, 50));
            var length = b1.Length - 8;
            const int offset = 4;
            var handle = new BlobHandle(b1, length, offset);
            
            Assert.False(k_Dictionary.ContainsKey(handle));
            k_Dictionary.Add(handle, k_Value);
            
            Assert.True(k_Dictionary.TryGetValue(handle, out var value));
            Assert.True(k_Dictionary.TryGetValueFromBytes(b1, length, offset, out var valueFromBytes));
            Assert.AreEqual(k_Value, value);    
            Assert.AreEqual(k_Value, valueFromBytes);    
        }
    }
}