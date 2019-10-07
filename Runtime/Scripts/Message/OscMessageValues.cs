#define OSCCORE_SAFETY_CHECKS
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

// allow tests to modify things as if in the same assembly
[assembly:InternalsVisibleTo("OscCore.Tests.Editor")]

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        // used to swap bytes for 32-bit numbers when reading
        readonly byte[] m_SwapBuffer32 = new byte[4];
        readonly byte* SwapBuffer32Ptr;
        readonly GCHandle m_Swap32Handle;
        // used to swap bytes for 64-bit numbers when reading
        readonly byte[] m_SwapBuffer64 = new byte[8];
        readonly byte* SwapBuffer64Ptr;
        readonly GCHandle m_Swap64Handle;
        // the buffer where we read messages from - usually provided + filled by a socket reader
        readonly byte[] m_SharedBuffer;
        readonly byte* SharedBufferPtr;
        readonly GCHandle m_BufferHandle;

        /// <summary>
        /// All type tags in the message.
        /// All values past index >= ElementCount are junk data and should NEVER BE USED!
        /// </summary>
        internal readonly TypeTag[] Tags;
        
        /// <summary>
        /// Indexes into the shared buffer associated with each message element
        /// All values at index >= ElementCount are junk data and should NEVER BE USED!
        /// </summary>
        internal readonly int[] Offsets;
        
        /// <summary>The number of elements in the OSC Message</summary>
        public int ElementCount { get; internal set; }

        internal OscMessageValues(byte[] buffer, int initialCapacity = 4)
        {
            ElementCount = 0;
            Tags = new TypeTag[initialCapacity];
            Offsets = new int[initialCapacity];
            m_SharedBuffer = buffer;

            // pin all buffers in place, so that we can count on the pointers never changing
            m_BufferHandle = GCHandle.Alloc(m_SharedBuffer, GCHandleType.Pinned);
            SharedBufferPtr = (byte*) m_BufferHandle.AddrOfPinnedObject();

            m_Swap32Handle = GCHandle.Alloc(m_SwapBuffer32, GCHandleType.Pinned);
            m_Swap64Handle = GCHandle.Alloc(m_SwapBuffer64, GCHandleType.Pinned);
            SwapBuffer32Ptr = (byte*) m_Swap32Handle.AddrOfPinnedObject();
            SwapBuffer64Ptr = (byte*) m_Swap64Handle.AddrOfPinnedObject();
        }

        ~OscMessageValues()
        {
            m_BufferHandle.Free();
            m_Swap32Handle.Free();
            m_Swap64Handle.Free();
        }

        /// <summary>Execute a method for every element in the message</summary>
        /// <param name="elementAction">A method that takes in the index and type tag for an element</param>
        public void ForEachElement(Action<int, TypeTag> elementAction)
        {
            for (int i = 0; i < ElementCount; i++)
                elementAction(i, Tags[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float ReadFloat32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadInt32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset]) return *ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MidiMessage ReadMidi(byte[] bytes, int offset)
        {
            return new MidiMessage(bytes, offset);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MidiMessage ReadMidiUnsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                return *(MidiMessage*) ptr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color32 ReadColor32(byte[] bytes, int offset)
        {
            var r = bytes[offset];
            var g = bytes[offset + 1];
            var b = bytes[offset + 2];
            var a = bytes[offset + 3];
            return new Color32(r, g, b, a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color32 ReadColor32Unsafe(byte[] bytes, int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                return *(Color32*) ptr;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static OscBlob ReadBlob(byte[] bytes, int offset)
        {
            return new OscBlob(bytes, offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static char ReadAsciiChar32(byte[] bytes, int offset)
        {
            return (char) bytes[offset];
        }
                
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ReadTrueTag(byte[] bytes, int offset) { return true; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ReadFalseTag(byte[] bytes, int offset) { return false; }
    }
}