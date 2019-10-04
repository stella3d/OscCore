using System;

namespace OscCore
{
    public class OscMessage
    {
        internal byte[] SharedBuffer;
        
        public Buffer<TypeTag, int> TagsToOffset;

        public void ReadAll()
        {
            var tags = TagsToOffset.Keys;
            var offsets = TagsToOffset.Values;
            for (int i = 0; i < TagsToOffset.Count; i++)
            {    
                var tag = tags[i];
                var offset = offsets[i];
                switch (tag)
                {
                    case TypeTag.False:
                        OscParser.ReadFalseTag(SharedBuffer, offset); break;
                    case TypeTag.True:
                        OscParser.ReadTrueTag(SharedBuffer, offset); break;
                    // no bytes or return values for these 2
                    case TypeTag.Infinitum:        
                    case TypeTag.Nil:
                        break;
                    case TypeTag.Blob:
                        OscParser.ReadBlob(SharedBuffer, offset); break;
                    case TypeTag.AsciiChar32:
                        OscParser.ReadAsciiChar32(SharedBuffer, offset); break;
                    case TypeTag.Float64:
                        OscParser.ReadFloat64Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Float32:
                        OscParser.ReadFloat32Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Int64:
                       OscParser.ReadInt64Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Int32:
                        OscParser.ReadInt32Unsafe(SharedBuffer, offset); break;
                    case TypeTag.MIDI:
                        OscParser.ReadMidiUnsafe(SharedBuffer, offset); break;
                    case TypeTag.Color32:
                        OscParser.ReadColor32Unsafe(SharedBuffer, offset); break;
                }
            }
        }
    }
}