#define OSCCORE_SAFETY_CHECKS

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OscCore
{
    public unsafe class OscValueHandle
    {
        readonly byte* BufferPtr;
        
        public TypeTag[] Tags;
        public int[] Offsets;
        
        public int ElementCount { get; internal set; }

        public OscValueHandle(byte* bufferPtr, int initialCapacity = 4)
        {
            BufferPtr = bufferPtr;
            ElementCount = 0;
            Tags = new TypeTag[initialCapacity];
            Offsets = new int[initialCapacity];
        }

        public void Resize(int newSize)
        {
            if (newSize <= Tags.Length)
                return;
            
            Array.Resize(ref Tags, newSize);
            Array.Resize(ref Offsets, newSize);
        }

        public float ReadFloatElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogWarning($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Float32:
                    return BufferPtr[offset];
                case TypeTag.Int32:
                    int i = BufferPtr[offset];
                    return i;
            }
            
            return default;
        }
        
        public int ReadIntElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogWarning($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.Int32:
                    return BufferPtr[offset];
                case TypeTag.Float32:
                    float f = BufferPtr[offset];
                    return (int) f;
            }
            
            return default;
        }
        
        public string ReadStringElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogWarning($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                // TODO - proper ReadString (element) method
                case TypeTag.String:
                    return ""; 
                case TypeTag.Float32:
                    float f = BufferPtr[offset];
                    return f.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Int32:
                    int i = BufferPtr[offset];
                    return i.ToString(CultureInfo.CurrentCulture);
            }

            return string.Empty;
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
        public static NtpTimestamp ReadTimestamp(byte[] bytes, int offset)
        {
            var seconds = BitConverter.ToUInt32(bytes, offset);
            var fractions = BitConverter.ToUInt32(bytes, offset + 4);
            return new NtpTimestamp(seconds, fractions);
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
        public static bool ReadTrueTag(byte[] bytes, int offset) { return true; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadFalseTag(byte[] bytes, int offset) { return false; }
    }
}