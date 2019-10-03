namespace OscCore
{
    // type tags from http://opensoundcontrol.org/spec-1_0 
    public enum TypeTag : byte
    {
        False = 70,                     // F, non-standard
        Infinitum = 78,                 // I, non-standard
        Nil = 78,                       // N, non-standard
        AltTypeString = 84,             // S, non-standard
        True = 84,                      // T, non-standard
        ArrayStart = 91,                // [, non-standard
        ArrayEnd = 93,                  // ], non-standard
        Blob = 98,                      // b, STANDARD
        AsciiChar32 = 99,               // c, non-standard
        Double = 100,                   // d, non-standard
        Float32 = 102,                  // f, STANDARD
        Long = 104,                     // h, non-standard
        Int32 = 105,                    // i, STANDARD
        MIDI = 109,                     // m, non-standard
        Color32 = 114,                  // r, non-standard
        String = 115                    // s, STANDARD
    }
}

