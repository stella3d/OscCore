using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ByteStrings;
using UnityEngine;

namespace OscCore
{
    public unsafe class OscParser
    {
        public const int BufferSize = 1024 * 8;

        static GCHandle BufferHandle;
        
        readonly byte[] SelfBuffer = new byte[BufferSize];

        byte* BufferPtr;

        static readonly Buffer<TypeTag> k_TagBuffer = new Buffer<TypeTag>(16);

        static OscMessageValues m_MessageValues;

        public OscParser()
        {
            // TODO - initial capacity option ?
            m_MessageValues = new OscMessageValues(SelfBuffer, 8);
            BufferHandle = GCHandle.Alloc(SelfBuffer, GCHandleType.Pinned);
            BufferPtr = (byte*) BufferHandle.AddrOfPinnedObject();
        }

        ~OscParser()
        {
            BufferHandle.Free();
        }

        public static void Parse(byte[] buffer, int length)
        {
            var addressLength = FindAddressLength(buffer, 0);
            var alignedAddressLength = addressLength.Align4();

            var debugStr = Encoding.ASCII.GetString(buffer, 0, addressLength);
            Debug.Log($"parsed address: {debugStr}");

            k_TagBuffer.Count = 0;
            ParseTags(buffer, alignedAddressLength, k_TagBuffer);

            Debug.Log("tag count: " + k_TagBuffer.Count);
            
            
        }
        
        public static void ParseToByteString(byte[] buffer, int length)
        {
            var addressLength = FindAddressLength(buffer, 0);
            var alignedAddressLength = addressLength.Align4();

            var debugStr = Encoding.ASCII.GetString(buffer, 0, addressLength);
            Debug.Log($"parsed address: {debugStr}");

            k_TagBuffer.Count = 0;
            ParseTags(buffer, alignedAddressLength, k_TagBuffer);

            Debug.Log("tag count: " + k_TagBuffer.Count);
            var bs = new ByteString();
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadPointer<T>(byte* bufferStartPtr, int offset)
            where T: unmanaged
        {
            return *(T*) (bufferStartPtr + offset);
        }

        /// <summary>
        /// Validate an OSC Address' name.
        /// </summary>
        /// <param name="address">The address of an OSC method</param>
        /// <returns>true if the address is valid, false otherwise</returns>
        public bool AddressIsValid(string address)
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
        
        /// <summary>
        /// Validate an OSC Address Pattern.
        /// </summary>
        /// <param name="address">An address pattern for an OSC method</param>
        /// <returns>true if the address pattern is valid, false otherwise</returns>
        public bool AddressPatternIsValid(string address)
        {
            if (address[0] != '/') return false;
            
            foreach (var chr in address)
            {
                switch (chr)
                {
                    case ' ':
                    case '#':
                    case ',':
                        return false;
                }
            }

            return true;
        }

        public static void ParseTags(byte[] bytes, int start, Buffer<TypeTag> tags)
        {
            tags.Count = 0;
            var tagIndex = start + 1;         // skip the starting ','

            var outIndex = 0;
            var outArray = tags.Array;
            while (true)
            {
                var tag = (TypeTag) bytes[tagIndex];
                if (!tag.IsSupported()) break;
                outArray[outIndex] = tag;
                tagIndex++;
                outIndex++; 
            }

            tags.Count = outIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ByteString ReadNewByteStringAddress(byte[] bytes, int offset)
        {
            var length = FindAddressLength(bytes, offset);
            return length == -1 ? default : new ByteString(bytes, length, offset);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindArrayLength(byte[] bytes, int offset)
        {
            if ((TypeTag) bytes[offset] != TypeTag.ArrayStart)
                return -1;
            
            var index = offset + 1;
            while (bytes[index] != (byte) TypeTag.ArrayEnd)
                index++;

            return index - offset;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FindAddressLength(byte[] bytes, int offset)
        {
            if (bytes[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;

            byte b = bytes[index];
            // we don't support lacking a type tag string
            while (b != byte.MinValue && b != Constant.Comma)
            {
                b = bytes[index];
                index++;
            }

            return index - offset;
        }
        
        // modified from the best answer at
        // https://stackoverflow.com/questions/11660127/faster-way-to-swap-endianness-in-c-sharp-with-32-bit-words
        public static unsafe void SwapX4(byte* Source, int offset = 0, int length = 4)
        {
            var bp = Source + offset;
            byte* bp_stop = bp + length;  

            while (bp < bp_stop)  
            {
                *(uint*)bp = (uint)(
                    (*bp       << 24) |
                    (*(bp + 1) << 16) |
                    (*(bp + 2) <<  8) |
                    (*(bp + 3)      ));
                bp += 4;  
            }  
        }

        public static unsafe int ReadBigEndianInt32Unsafe(byte* Source, int offset = 0)
        {
            var bp = Source + offset;

            *(int*)bp = (
                (*bp       << 24) |
                (*(bp + 1) << 16) |
                (*(bp + 2) <<  8) |
                (*(bp + 3)      ));

            return *bp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadBigEndianInt(byte[] buffer, int offset)
        {
            return buffer[offset    ] << 24 |
                   buffer[offset + 1] << 16 |
                   buffer[offset + 2] <<  8 |
                   buffer[offset + 3];
        }
    }
}

