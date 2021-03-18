using System;
using System.Linq;
using System.Text;
using Random = UnityEngine.Random;
using SystemRandom = System.Random;

namespace OscCore.Tests
{
    public static class OscRandom
    {
        static readonly byte[] StandardTypeTags = new byte[] { 98, 102, 105, 115 };
        static readonly byte[] NonStandardTypeTags = new byte [] { 
            70, 73, 78, 84, 85, 91, 93, 99, 100, 104, 109, 114, 116 };

        static readonly byte[] AllTypeTags = StandardTypeTags.Concat(NonStandardTypeTags).ToArray();

        internal static SystemRandom s_SystemRand = new SystemRandom();


        public static OscMessage GetSingleElementMessage(bool useNonstandardTypes = false)
        {
            var addr = GetAddress();
            var tagByte = GetTypeTag(useNonstandardTypes);
            var tagStr = "," + (char) tagByte;
            var data = GetElementData((TypeTag) tagByte);
            return new OscMessage(addr, tagStr, data); 
        }

        public static void GenerateBundle()
        {
            
        }

        internal static byte[] GetElementData(TypeTag tag)
        {
            byte[] data;
            switch(tag) {
                case TypeTag.Blob:
                    var bLen = Random.Range(8, 256);
                    var alignedBlobLen = (bLen + 3) & ~3;
                    data = new byte[alignedBlobLen];
                    s_SystemRand.NextBytes(data);
                    break;
                case TypeTag.Float32:
                    data = BitConverter.GetBytes(Random.Range(float.MinValue, float.MaxValue));
                    break;
                case TypeTag.Int32:
                    data = BitConverter.GetBytes(Random.Range(int.MinValue, int.MaxValue));
                    break;
                case TypeTag.String:
                    var sLen = Random.Range(8, 256);
                    var alignedStrLen = (sLen + 3) & ~3;
                    data = new byte[alignedStrLen];
                    s_SystemRand.NextBytes(data);
                    data[alignedStrLen - 1] = 0;         // add str terminator
                    break;
            }
        }

        static readonly StringBuilder k_AddressBuilder = new StringBuilder();

        public static string GetAddress(int maxPartLength = 10)
        {
            k_AddressBuilder.Length = 0;
            const int charStart = 65; // from which ascii character code the generation should start
            const int charEnd = 122; // to which ascii character code the generation should end

            var partCount = Random.Range(1, 5);
            for(var i = 0; i < partCount; i++)
            {
                k_AddressBuilder.Append('/');
                var pLen = Random.Range(2, maxPartLength);
                for (int i = 0; i < characterCount; i++)
                    builder.Append((char)(Random.Range(charStart, charEnd + 1) % 255));
            }

            return k_AddressBuilder.ToString();
        }

        internal static TypeTag GetTypeTag(bool useNonstandardTypes = false)
        {
            var possibleTags = useNonstandardTypes ? AllTypeTags : StandardTypeTags;
            return (TypeTag) possibleTags.GetRandomElement();
        }
    }

    public static class Extensions
    {
        public static T GetRandomElement<T>(this T[] a) => a[Random.Range(0, a.Length - 1)];
    }
}