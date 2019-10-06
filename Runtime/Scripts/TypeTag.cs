using System.Runtime.CompilerServices;

namespace OscCore
{
    // type tags from http://opensoundcontrol.org/spec-1_0 
    public enum TypeTag : byte
    {
        False = 70,                     // F, non-standard
        Infinitum = 73,                 // I, non-standard
        Nil = 78,                       // N, non-standard
        AltTypeString = 84,             // S, non-standard
        True = 84,                      // T, non-standard
        ArrayStart = 91,                // [, non-standard
        ArrayEnd = 93,                  // ], non-standard
        Blob = 98,                      // b, STANDARD
        AsciiChar32 = 99,               // c, non-standard
        Float64 = 100,                  // d, non-standard
        Float32 = 102,                  // f, STANDARD
        Int64 = 104,                    // h, non-standard
        Int32 = 105,                    // i, STANDARD
        MIDI = 109,                     // m, non-standard
        Color32 = 114,                  // r, non-standard
        String = 115,                   // s, STANDARD
        TimeTag = 116                   // t, non-standard
    }

    public static class TypeTagMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSupported(this TypeTag tag)
        {
            switch (tag)
            {
                case TypeTag.False: return true;
                case TypeTag.Infinitum: return true;
                case TypeTag.Nil: return true;
                case TypeTag.True: return true;
                case TypeTag.Blob: return true;
                case TypeTag.AsciiChar32: return true;
                case TypeTag.Float64: return true;
                case TypeTag.Float32: return true;
                case TypeTag.Int64: return true;
                case TypeTag.Int32: return true;
                case TypeTag.String: return true;        
                case TypeTag.MIDI: return true;
                case TypeTag.Color32: return true;
                default: return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteToPointer(this TypeTag tag, byte* bufferPtr, int offset)
        {
            *(bufferPtr + offset) = (byte) tag;
        }
    }
}

