using System.Runtime.CompilerServices;
using ByteStrings;
using Unity.Collections;
using UnityEngine;

namespace OscCore
{
    public class OscParser
    {
        /// <summary>
        /// Validate an OSC Address' name.
        /// This will reject valid Address Patterns, use 
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
        public static unsafe double ReadFloat64Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ReadFloat32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe long ReadInt64Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ReadInt32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MidiMessage ReadMidi(byte[] bytes, int offset)
        {
            return new MidiMessage(bytes, offset);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MidiMessage ReadMidiUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                return *(MidiMessage*) ptr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe T ReadUnsafe<T>(byte[] bytes, int offset)
            where T: unmanaged
        {
            fixed (byte* ptr = &bytes[offset]) return *(T*) ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NtpTimestamp ReadTimestampUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                return *(NtpTimestamp*) ptr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 ReadColor32(byte[] bytes, int offset)
        {
            var r = bytes[offset];
            var g = bytes[offset + 1];
            var b = bytes[offset + 2];
            var a = bytes[offset + 3];
            return new Color32(r, g, b, a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Color32 ReadColor32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                return *(Color32*) ptr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OscBlob ReadBlob(byte[] bytes, int offset)
        {
            return new OscBlob(bytes, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadAsciiChar32(byte[] bytes, int offset)
        {
            return (char) bytes[offset];
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
            // we don't support lacking a type tag string
            while (bytes[index] != Constant.Comma)
                index++;

            return index - offset;
        }

        // the methods below are here to keep the pattern with all other types, despite returning constant values.
        // We want a method associated with reading each type tag.
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadTrueTag(byte[] bytes, int offset) { return true; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadFalseTag(byte[] bytes, int offset) { return false; }
    }
}

