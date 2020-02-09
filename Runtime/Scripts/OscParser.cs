using System.Runtime.CompilerServices;

namespace OscCore
{
    public unsafe class OscParser
    {
        // TODO - make these preferences options
        public const int MaxElementsPerMessage = 32;
        public const int MaxBlobSize = 1024 * 256;

        internal readonly byte[] Buffer;
        internal readonly byte* BufferPtr;
        internal readonly long* BufferLongPtr;

        public readonly OscMessageValues MessageValues;

        /// <summary>Create a new parser.</summary>
        /// <param name="fixedBuffer">The buffer to read messages from.  Must be fixed in memory !</param>
        public OscParser(byte[] fixedBuffer)
        {
            Buffer = fixedBuffer;
            fixed (byte* ptr = fixedBuffer)
            {
                BufferPtr = ptr;
                BufferLongPtr = (long*) ptr;
            }
            MessageValues = new OscMessageValues(Buffer, MaxElementsPerMessage);
        }
        
        public int Parse(int byteLength)
        {
            var buffer = Buffer;
            var bufferPtr = BufferPtr;
            var bufferLongPtr = BufferLongPtr;
            
            // determine if the message is a bundle or not 
            if (*bufferLongPtr != Constant.BundlePrefixLong)
            {
                // address length here doesn't include the null terminator and alignment padding.
                // this is so we can look up the address by only its content bytes.
                var addressLength = FindUnalignedAddressLength();
                if (addressLength < 0)
                    return addressLength;    // address didn't start with '/'

                var alignedAddressLength = (addressLength + 3) & ~3;
                // if the null terminator after the string comes at the beginning of a 4-byte block,
                // we need to add 4 bytes of padding
                if (alignedAddressLength == addressLength)
                    alignedAddressLength += 4;

                var tagCount = ParseTags(buffer, alignedAddressLength);
                var offset = alignedAddressLength + (tagCount + 4) & ~3;
                FindOffsets(offset);
                return addressLength;
            }

            // the message is a bundle, so we need to recursively scan the bundle elements
            int MessageOffset = 0;
            bool recurse;
            // the outer do-while loop runs once for every #bundle encountered
            do
            {
                // Timestamp isn't used yet, but it will be eventually
                // var time = parser.MessageValues.ReadTimestampIndex(MessageOffset + 8);
                // '#bundle ' + timestamp = 16 bytes
                MessageOffset += 16;
                recurse = false;

                // the inner while loop runs once per bundle element
                while (MessageOffset < byteLength && !recurse)
                {
                    var messageSize = (int) MessageValues.ReadUIntIndex(MessageOffset);
                    var contentIndex = MessageOffset + 4;

                    if (IsBundleTagAtIndex(contentIndex))
                    {
                        // this bundle element's contents are a bundle, break out to the outer loop to scan it
                        MessageOffset = contentIndex;
                        recurse = true;
                        continue;
                    }

                    var bundleAddressLength = FindUnalignedAddressLength(contentIndex);
                    if (bundleAddressLength <= 0)
                    {
                        // if an error occured parsing the address, skip this message entirely
                        MessageOffset += messageSize + 4;
                        continue;
                    }

                    var bundleTagCount = ParseTags(buffer, contentIndex + bundleAddressLength);
                    if (bundleTagCount <= 0)
                    {
                        MessageOffset += messageSize + 4;
                        continue;
                    }

                    // skip the ',' and align to 4 bytes
                    var bundleOffset = (contentIndex + bundleAddressLength + bundleTagCount + 4) & ~3;
                    FindOffsets(bundleOffset);

                    MessageOffset += messageSize + 4;
                }
            }
            // restart the outer while loop every time a bundle within a bundle is detected
            while (recurse);

            // TODO - maybe we can't put bundle and non-bundle in the same func ?  this isn't right to return
            return MessageOffset;
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
        
        public static bool CharacterIsValidInAddress(char c)
        {
            switch (c)
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
                default:
                    return true;
            }
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

        public static int FindArrayLength(byte[] bytes, int offset = 0)
        {
            if ((TypeTag) bytes[offset] != TypeTag.ArrayStart)
                return -1;
            
            var index = offset + 1;
            while (bytes[index] != (byte) TypeTag.ArrayEnd)
                index++;

            return index - offset;
        }
        
        public static int FindUnalignedAddressLength(byte[] bytes, int offset = 0)
        {
            if (bytes[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;

            byte b = bytes[index];
            while (b != byte.MinValue)
            {
                b = bytes[index];
                index++;
            }

            var length = index - offset;
            return length;
        }
        
        public int FindUnalignedAddressLength()
        {
            if (BufferPtr[0] != Constant.ForwardSlash)
                return -1;
            
            var index = 0;
            do
            {
                index++;
            } 
            while (BufferPtr[index] != byte.MinValue);
            return index;
        }
        
        public int FindUnalignedAddressLength(int offset)
        {
            if (BufferPtr[offset] != Constant.ForwardSlash)
                return -1;
            
            var index = offset + 1;
            do
            {
                index++;
            } 
            while (BufferPtr[index] != byte.MinValue);

            var length = index - offset;
            return length;
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
                    // false, true, nil, infinitum & array[] tags add 0 to the offset
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBundleTagAtIndex(int index)
        {
            return *((long*) BufferPtr + index) == Constant.BundlePrefixLong;
        }
    }
}

