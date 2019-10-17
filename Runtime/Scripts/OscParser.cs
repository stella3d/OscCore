using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BlobHandles;
using UnityEngine;

namespace OscCore
{
    public unsafe class OscParser : IDisposable
    {
        internal static readonly long BundleStringValue = BundleStringAsLong();    // "#bundle " 
        
        // TODO - make these preferences options
        public const int MaxElementsPerMessage = 32;
        public const int MaxBlobSize = 1024 * 256;

        internal readonly byte[] Buffer;
        readonly byte* BufferPtr;
        
        /// <summary>
        /// Pointer to the first 8 bytes of the read buffer.
        /// Used to determine if a message is a bundle in a single comparison
        /// </summary>
        internal readonly long* BufferLongPtr;

        public readonly OscMessageValues MessageValues;

        public int TagCount { get; private set; }

        public OscAddressSpace AddressSpace { get; internal set; }

        public OscParser(byte[] fixedBuffer, GCHandle bufferHandle)
        {
            Buffer = fixedBuffer;
            fixed (byte* ptr = fixedBuffer)
            {
                BufferPtr = ptr;
                BufferLongPtr = (long*) ptr;
            }
            MessageValues = new OscMessageValues(Buffer, bufferHandle, MaxElementsPerMessage);
        }

        public static void Parse(byte[] buffer, int length)
        {
            var addressLength = FindAddressLength(buffer, 0);
            var debugStr = Encoding.ASCII.GetString(buffer, 0, addressLength);
            Debug.Log($"parsed address: {debugStr}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddressIsBundle()
        {
            return *BufferLongPtr == BundleStringValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadPointer<T>(byte* bufferStartPtr, int offset)
            where T: unmanaged
        {
            return *(T*) (bufferStartPtr + offset);
        }

        internal static bool AddressIsValid(string address)
        {
            if (address[0] != '/') return false;

            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case '*':
                    case ',':
                    case '?':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        return false;
                }
            }

            return true;
        }

        internal static AddressType GetAddressType(string address)
        {
            if (address[0] != '/') return AddressType.Invalid;

            var addressValid = true;
            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case '*':
                    case ',':
                    case '?':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        addressValid = false;
                        break;
                }
            }

            if (addressValid) return AddressType.Address;
            
            // if the address isn't valid, it might be a valid address pattern.
            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case ',':
                        return AddressType.Invalid;
                }
            }

            return AddressType.Pattern;
        }
        
        public int ParseTags(byte[] bytes, int start = 0)
        {
            if (bytes[start] != Constant.Comma) return 0;
            
            var tagIndex = start + 1;         // skip the starting ','
            var outIndex = 0;
            var tags = MessageValues.Tags;
            while (true)
            {
                var tag = (TypeTag) bytes[tagIndex];
                if (!tag.IsSupported()) break;
                tags[outIndex] = tag;
                tagIndex++;
                outIndex++; 
            }

            MessageValues.ElementCount = outIndex;
            return outIndex;
        }
        
        public int ParseTags(int start = 0)
        {
            if (Buffer[start] != Constant.Comma) return 0;
            
            var tagIndex = start + 1;         // skip the starting ','
            var outIndex = 0;
            var tags = MessageValues.Tags;
            while (true)
            {
                var tag = (TypeTag) Buffer[tagIndex];
                if (!tag.IsSupported()) break;
                tags[outIndex] = tag;
                tagIndex++;
                outIndex++; 
            }

            MessageValues.ElementCount = outIndex;
            return outIndex;
        }
        
        public static int FindArrayLength(byte[] bytes, int offset = 0)
        {
            if ((TypeTag) bytes[offset] != TypeTag.ArrayStart)
                return -1;
            
            var index = offset + 1;
            while (bytes[index] != (byte) TypeTag.ArrayEnd)
                index++;

            return index - offset;
        }
        
        public static int FindAddressLength(byte[] bytes, int offset = 0)
        {
            if (bytes[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;

            byte b = bytes[index];
            while (b != byte.MinValue && b != Constant.Comma)
            {
                b = bytes[index];
                index++;
            }

            var length = index - offset;
            return (length + 3) & ~3;            // align to 4 bytes
        }
        
        public int FindAddressLength(int offset = 0)
        {
            var buffer = Buffer;
            if (buffer[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;

            byte b = buffer[index];
            while (b != byte.MinValue && b != Constant.Comma)
            {
                b = buffer[index];
                index++;
            }

            var length = index - offset;
            return (length + 3) & ~3;            // align to 4 bytes
        }

        public int GetStringLength(int offset)
        {
            var end = Buffer.Length - offset;
            int index;
            for (index = offset; index < end; index++)
            {
                if (Buffer[index] != 0) break;
            }

            var length = index - offset;
            return (length + 3) & ~3;            // align to 4 bytes
        }

        public void FindOffsets(int offset)
        {
            var tags = MessageValues.Tags;
            var offsets = MessageValues.Offsets;
            for (int i = 0; i < MessageValues.ElementCount; i++)
            {
                offsets[i] = offset;
                switch (tags[i])
                {
                    // false, true, nil & infinitum tags add 0 to the offset
                    case TypeTag.Int32:
                    case TypeTag.Float32:
                    case TypeTag.Color32:    
                    case TypeTag.AsciiChar32:
                    case TypeTag.MIDI:
                        offset += 4; 
                        break;
                    case TypeTag.Float64:
                    case TypeTag.Int64:
                    case TypeTag.TimeTag:
                        offset += 8;
                        break;
                    case TypeTag.String:
                    case TypeTag.AltTypeString:
                        offset += GetStringLength(offset);
                        break;
                    case TypeTag.Blob:
                        // read the int that specifies the size of the blob
                        offset += 4 + *(int*)(BufferPtr + offset);
                        break;
                }
            }
        }

        public bool TryParseMessage()
        {
            var addressLength = FindAddressLength();
            if (addressLength < 0) 
                return false;

            var tagCount = ParseTags(addressLength);
            if (tagCount > 0)
            {
                // skip the ',' and align to 4 bytes
                var tagByteLength = ((tagCount + 1) + 3) & ~3;            
                FindOffsets(addressLength + tagByteLength);
                return true;
            }

            return false;
        }
        
        static long BundleStringAsLong()
        {
            var bundleBytes = Encoding.ASCII.GetBytes("#bundle ");
            bundleBytes[7] = 0;
            return BitConverter.ToInt64(bundleBytes, 0);
        }

        public void Dispose()
        {
        }
    }
}

