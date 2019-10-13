using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlobHandles;

namespace OscCore
{
    enum AddressType
    {
        Invalid,
        Pattern,
        Address
    }

    public sealed class OscAddressSpace
    {
        const int k_DefaultPatternCapacity = 8;
        const int k_DefaultCapacity = 16;

        internal readonly OscAddressMethods AddressToMethod;
        
        // Keep a list of registered address patterns and the methods they're associated with just like addresses
        internal int PatternCount;
        internal Regex[] Patterns = new Regex[k_DefaultPatternCapacity];
        internal ReceiveValueMethod[] PatternMethods = new ReceiveValueMethod[k_DefaultPatternCapacity];
        
        readonly Queue<int> FreedPatternIndices = new Queue<int>();
        readonly Dictionary<string, int> PatternStringToIndex = new Dictionary<string, int>();

        public OscAddressSpace(int startingCapacity = k_DefaultCapacity)
        {
            AddressToMethod = new OscAddressMethods(startingCapacity);
        }

        public bool TryAddMethod(string address, ReceiveValueMethod onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    AddressToMethod.Add(address, onReceived);
                    return true;
                case AddressType.Pattern:
                    int index;
                    // if a method has already been registered for this pattern, add the new delegate
                    if (PatternStringToIndex.TryGetValue(address, out index))
                    {
                        PatternMethods[index] += onReceived;
                        return true;
                    }

                    if (FreedPatternIndices.Count > 0)
                    {
                        index = FreedPatternIndices.Dequeue();
                    }
                    else
                    {
                        index = PatternCount;
                        if (index >= Patterns.Length)
                        {
                            var newSize = Patterns.Length * 2;
                            Array.Resize(ref Patterns, newSize);
                            Array.Resize(ref PatternMethods, newSize);
                        }
                    }

                    Patterns[index] = new Regex(address);
                    PatternMethods[index] = onReceived;
                    PatternStringToIndex[address] = index;
                    PatternCount++;
                    return true;
                default: 
                    return false;
            }
        }

        public bool RemoveMethod(string address, ReceiveValueMethod onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    AddressToMethod.Remove(address, onReceived);
                    return true;
                case AddressType.Pattern:
                    if (!PatternStringToIndex.TryGetValue(address, out var patternIndex))
                        return false;

                    var method = PatternMethods[patternIndex];
                    if (method.GetInvocationList().Length == 1)
                    {
                        Patterns[patternIndex] = null;
                        PatternMethods[patternIndex] = null;
                    }
                    else
                    {
                        PatternMethods[patternIndex] -= onReceived;
                    }

                    PatternCount--;
                    FreedPatternIndices.Enqueue(patternIndex);
                    return PatternStringToIndex.Remove(address);
                default: 
                    return false;
            }
        }

        public bool TryMatchPattern(string address, out ReceiveValueMethod method)
        {
            for (var i = 0; i < PatternCount; i++)
            {
                if (Patterns[i].IsMatch(address))
                {
                    method = PatternMethods[i];
                    return true;
                }
            }

            method = default;
            return false;
        }
        
        bool AddAddressMethod(string address, ReceiveValueMethod onReceived)
        {
            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    AddressToMethod.Add(address, onReceived);
                    return true;
                case AddressType.Pattern:
                    int index;
                    // if a method has already been registered for this pattern, add the new delegate
                    if (PatternStringToIndex.TryGetValue(address, out index))
                    {
                        PatternMethods[index] += onReceived;
                        return true;
                    }

                    if (FreedPatternIndices.Count > 0)
                    {
                        index = FreedPatternIndices.Dequeue();
                    }
                    else
                    {
                        index = PatternCount;
                        if (index >= Patterns.Length)
                        {
                            var newSize = Patterns.Length * 2;
                            Array.Resize(ref Patterns, newSize);
                            Array.Resize(ref PatternMethods, newSize);
                        }
                    }

                    Patterns[index] = new Regex(address);
                    PatternMethods[index] = onReceived;
                    PatternStringToIndex[address] = index;
                    PatternCount++;
                    return true;
                default: 
                    return false;
            }
        }
        
        /// <summary>
        /// Try to match an address against all known address patterns,
        /// and add a handler for the address if a pattern is matched
        /// </summary>
        /// <param name="address">The address to match</param>
        /// <returns>True if a match was found, false otherwise</returns>
        public bool TryMatchPatternHandler(string address, out ReceiveValueMethod handler)
        {
            if (!OscParser.AddressIsValid(address))
            {
                handler = default;
                return false;
            }
            
            for (var i = 0; i < PatternCount; i++)
            {
                if (Patterns[i].IsMatch(address))
                {
                    handler = PatternMethods[i];
                    AddressToMethod.Add(address, handler);
                    return true;
                }
            }

            handler = default;
            return false;
        }
    }
}

