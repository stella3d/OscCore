using System;
using Random = UnityEngine.Random;

namespace OscCore.Tests
{
    public static class TestUtil
    {
        public static byte[] ReversedCopy(byte[] source)
        {
            var copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            Array.Reverse(copy);
            return copy;
        }
        
        public static byte[] RandomFloatBytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var f = Random.Range(-1f, 1f);
                var fBytes = BitConverter.GetBytes(f);
                for (int j = 0; j < fBytes.Length; j++)
                {
                    bytes[i + j] = fBytes[j];
                }
            }

            return bytes;
        }
        
        public static byte[] RandomIntBytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var iValue = Random.Range(-1000, 1000);
                var iBytes = BitConverter.GetBytes(iValue);
                for (int j = 0; j < iBytes.Length; j++)
                    bytes[i + j] = iBytes[j];
            }

            return bytes;
        }
        
        public static byte[] RandomColor32Bytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var iValue = Random.Range(0, 255);
                var iBytes = BitConverter.GetBytes(iValue);
                for (int j = 0; j < iBytes.Length; j++)
                    bytes[i + j] = iBytes[j];
            }

            return bytes;
        }
        
        public static byte[] RandomTimestampBytes(int count = 2048)
        {
            var bytes = new byte[count];
            for (int i = 0; i < bytes.Length; i += 8)
            {
                var seconds = Random.Range(0, 255);
                var sBytes = BitConverter.GetBytes(seconds);
                for (int j = 0; j < sBytes.Length; j++)
                    bytes[i + j] = sBytes[j];
                
                var fractions = Random.Range(0, 10000000);
                var fBytes = BitConverter.GetBytes(fractions);

                var end = 4 + fBytes.Length;
                for (int j = 4; j < end; j++)
                    bytes[i + j] = fBytes[j - 4];
            }

            return bytes;
        }

    }
}