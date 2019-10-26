using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OscCore;
using Unity.IL2CPP.CompilerServices;

namespace BlobHandles
{
    internal sealed unsafe class OscAddressMethods: IDisposable
    {
        const int defaultSize = 16;
        
        public readonly Dictionary<BlobHandle, OscActionPair> HandleToValue;
        internal readonly Dictionary<string, BlobString> SourceToBlob;

        public OscAddressMethods(int initialCapacity = defaultSize)
        {
            HandleToValue = new Dictionary<BlobHandle, OscActionPair>(initialCapacity);
            SourceToBlob = new Dictionary<string, BlobString>(initialCapacity);
        }
        
        /// <summary>Adds a callback to be executed when a message is received at the address</summary>
        /// <param name="address">The address to associate the method with</param>
        /// <param name="callbacks">The method(s) to be invoked</param>
        [Il2CppSetOption(Option.NullChecks, false)]
        public void Add(string address, OscActionPair callbacks)
        {
            if (!SourceToBlob.TryGetValue(address, out var blobStr))
            {
                blobStr = new BlobString(address);
                HandleToValue[blobStr.Handle] = callbacks;
                SourceToBlob.Add(address, blobStr);
            }
            else
            {
                if(HandleToValue.ContainsKey(blobStr.Handle))
                    HandleToValue[blobStr.Handle] += callbacks;
                else
                    HandleToValue[blobStr.Handle] = callbacks;
            }
        }
        
        /// <summary>Adds a callback to be executed when a message is received at the address</summary>
        /// <param name="address">The address to associate the method with</param>
        /// <param name="callback">The method to be invoked</param>
        [Il2CppSetOption(Option.NullChecks, false)]
        public void Add(string address, Action<OscMessageValues> callback)
        {
            Add(address, new OscActionPair(callback, null));
        }
        
        /// <summary>Adds a list of callbacks to be executed when a message is received at the address</summary>
        /// <param name="address">The address to associate the methods with</param>
        /// <param name="callbacks">The methods to be invoked</param>
        [Il2CppSetOption(Option.NullChecks, false)]
        internal void Add(string address, List<OscActionPair> callbacks)
        {
            if (callbacks.Count == 0) return;

            var pair = callbacks[0];
            if(callbacks.Count > 1)
                for (int i = 1; i < callbacks.Count; i++)
                    pair += callbacks[i];
            
            Add(address, pair);
        }

        /// <summary>Removes the callback at the specified address</summary>
        /// <param name="address">The address to remove</param>
        /// <param name="callback">The callback to remove</param>
        /// <returns>true if the string was found and removed, false otherwise</returns>
        [Il2CppSetOption(Option.NullChecks, false)]
        public bool Remove(string address, OscActionPair callbacks)
        {
            if (!SourceToBlob.TryGetValue(address, out var blobStr)) 
                return false;
            if (!HandleToValue.TryGetValue(blobStr.Handle, out var existingPair))
                return false;

            var valueReadMethod = existingPair.ValueRead;
            if (valueReadMethod.GetInvocationList().Length == 1)
            {
                var removed = HandleToValue.Remove(blobStr.Handle) && SourceToBlob.Remove(address);
                blobStr.Dispose();
                return removed;
            }
            
            HandleToValue[blobStr.Handle] -= callbacks;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Il2CppSetOption(Option.NullChecks, false)]
        public unsafe bool TryGetValueFromBytes(byte* ptr, int byteCount, out OscActionPair value)
        {
            var debugBlobStr = new BlobString(ptr, byteCount);
            debugBlobStr.Dispose();
            return HandleToValue.TryGetValue(new BlobHandle(ptr, byteCount), out value);
        }

        public void Clear()
        {
            HandleToValue.Clear();
            SourceToBlob.Clear();
        }

        public void Dispose()
        {
            foreach (var kvp in SourceToBlob)
                kvp.Value.Dispose();
        }
    }
}