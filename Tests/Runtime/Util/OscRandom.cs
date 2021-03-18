using System;
using System.Linq;
using System.Text;
using UnityEngine;
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

        public static OscMessageSequence GetSequence(int length, bool multiElementMessages = false, 
            bool nonStandardTypes = false, bool bundled = false)
        {
            var msgTime = 0f;
            TimedMessage[] messages;
            if(bundled) 
            {
                messages = null;    // TODO - implement
            }
            else 
            {
                messages = new TimedMessage[length];
                for(var i = 0; i < length; i++)
                {
                    if(multiElementMessages)
                    {
                        messages = null;    // TODO - implement
                        break;
                    }
                    else
                    {
                        var msg = OscRandom.GetSingleElementMessage(nonStandardTypes);
                        messages[i] = new TimedMessage(msgTime, msg); 
                    }

                    msgTime += TimeStep();
                }
            }

            var seq = (OscMessageSequence) ScriptableObject.CreateInstance(typeof(OscMessageSequence));
            seq.name = "New OSC Sequence";
            seq.Messages = messages;
            return seq;
        }

        static float TimeStep()
        {
            const float minStep = 0f;
            const float maxStep = 0.2f;
            var step = Random.Range(minStep, maxStep);
            return step < 0.001f ? 0f : step;
        }

        public static void GenerateBundle() { }

        static readonly byte[] k_EmptyBytes = new byte[0];

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
                default:
                    data = k_EmptyBytes;
                    break;
            }
            return data;
        }

        static readonly StringBuilder k_AddressBuilder = new StringBuilder();

        public static string GetAddress(int maxPartLength = 10)
        {
            k_AddressBuilder.Length = 0;
            // want to exclude some special chars in the middle
            const int cStart1 = 65; 
            const int cEnd1 = 90;
            const int cStart2 = 97; 
            const int cEnd2 = 122; 

            var partCount = Random.Range(1, 5);
            for(var i = 0; i < partCount; i++)
            {
                k_AddressBuilder.Append('/');
                var pLen = Random.Range(2, maxPartLength);
                for (int c = 0; c < pLen; c++)
                {
                    var rnd1 = Random.Range(-0.5f, 1f);
                    var nextChar = rnd1 < 0f ? Random.Range(cStart1, cEnd1) : Random.Range(cStart2, cEnd2); 
                    k_AddressBuilder.Append((char)nextChar);
                }
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