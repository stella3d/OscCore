using System;
using Random = UnityEngine.Random;

namespace OscCore.Tests
{
    public static class TestUtil
    {
        static readonly byte[] k_Swap32 = new byte[4];
        static readonly byte[] k_Swap64 = new byte[8];
        
        public static int ReverseBytes(this int self)
        {
            k_Swap32[0] = (byte) (self >> 24);
            k_Swap32[1] = (byte) (self >> 16);
            k_Swap32[2] = (byte) (self >>  8);
            k_Swap32[3] = (byte) (self);
            return BitConverter.ToInt32(k_Swap32, 0);
        }
        
        public static float ReverseBytes(this float self)
        {
            var fBytes = BitConverter.GetBytes(self);
            k_Swap32[0] = fBytes[3];
            k_Swap32[1] = fBytes[2];
            k_Swap32[2] = fBytes[1];
            k_Swap32[3] = fBytes[0];
            return BitConverter.ToSingle(k_Swap32, 0);
        }
        
        public static double ReverseBytes(this double self)
        {
            var dBytes = BitConverter.GetBytes(self);
            k_Swap64[0] = dBytes[7];
            k_Swap64[1] = dBytes[6];
            k_Swap64[2] = dBytes[5];
            k_Swap64[3] = dBytes[4];
            k_Swap64[4] = dBytes[3];
            k_Swap64[5] = dBytes[2];
            k_Swap64[6] = dBytes[1];
            k_Swap64[7] = dBytes[0];
            return BitConverter.ToDouble(k_Swap64, 0);
        }
        
        public static byte[] ReversedCopy(byte[] source)
        {
            var copy = new byte[source.Length];
            Array.Copy(source, copy, source.Length);
            Array.Reverse(copy);
            return copy;
        }
        
        public static byte[] RandomFloatBytes(int byteCount = 2048)
        {
            var bytes = new byte[byteCount];
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
        
        public static byte[] RandomIntBytes(int byteCount = 2048)
        {
            var bytes = new byte[byteCount];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var iValue = Random.Range(-1000, 1000);
                var iBytes = BitConverter.GetBytes(iValue);
                for (int j = 0; j < iBytes.Length; j++)
                    bytes[i + j] = iBytes[j];
            }

            return bytes;
        }
        
        public static byte[] RandomColor32Bytes(int byteCount = 2048)
        {
            var bytes = new byte[byteCount];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var iValue = Random.Range(0, 255);
                var iBytes = BitConverter.GetBytes(iValue);
                for (int j = 0; j < iBytes.Length; j++)
                    bytes[i + j] = iBytes[j];
            }

            return bytes;
        }
        
        public static byte[] RandomMidiBytes(int byteCount = 2048)
        {
            var bytes = new byte[byteCount];
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var port = (byte) Random.Range(1, 16);
                var status = (byte) Random.Range(0, 127);
                var data1 = (byte) Random.Range(10, 255);
                var data2 = (byte) Random.Range(10, 255);
                bytes[i] = port;
                bytes[i + 1] = status;
                bytes[i + 2] = data1;
                bytes[i + 3] = data2;
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