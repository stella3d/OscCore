using System.Linq;
using UnityEngine;

namespace BlobHandles.Tests
{
    public static class TestData
    {
        public static string[] RandomStrings(int count, int stringLengthMin, int stringLengthMax)
        {
            var strings = new string[count];
            for (int i = 0; i < strings.Length; i++)
                strings[i] = RandomString(stringLengthMin, stringLengthMax);

            return strings;
        }
        
        // helps test performance impact of strings that share a common beginning
        public static string[] RandomStringsWithPrefix(string prefix, int count, int stringLengthMin, int stringLengthMax)
        {
            var strings = new string[count];
            for (int i = 0; i < strings.Length; i++)
                strings[i] = prefix + RandomString(stringLengthMin, stringLengthMax);

            return strings;
        }

        public static string RandomString(int minLength, int maxLength)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ/0123456789";
            
            var length = Random.Range(minLength, maxLength);
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }

        public static class StringConstants
        {
            public const string EatTheRich = "Eat the rich";
            public const string M4A = "Medicare for all";
            public const string HealthJustice = "Health justice now!";
        }
    }
}