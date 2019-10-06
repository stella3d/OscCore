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
                        OscValueHandle.ReadFalseTag(SharedBuffer, offset); break;
                    case TypeTag.True:
                        OscValueHandle.ReadTrueTag(SharedBuffer, offset); break;
                    // no bytes or return values for these 2
                    case TypeTag.Infinitum:        
                    case TypeTag.Nil:
                        break;
                    case TypeTag.Blob:
                        OscValueHandle.ReadBlob(SharedBuffer, offset); break;
                    case TypeTag.AsciiChar32:
                        OscValueHandle.ReadAsciiChar32(SharedBuffer, offset); break;
                    case TypeTag.Float64:
                        OscValueHandle.ReadFloat64Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Float32:
                        OscValueHandle.ReadFloat32Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Int64:
                       OscValueHandle.ReadInt64Unsafe(SharedBuffer, offset); break;
                    case TypeTag.Int32:
                        OscValueHandle.ReadInt32Unsafe(SharedBuffer, offset); break;
                    case TypeTag.MIDI:
                        OscValueHandle.ReadMidiUnsafe(SharedBuffer, offset); break;
                    case TypeTag.Color32:
                        OscValueHandle.ReadColor32Unsafe(SharedBuffer, offset); break;
                }
            }
        }
    }
}