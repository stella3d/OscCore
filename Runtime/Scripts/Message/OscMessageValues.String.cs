using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit RGBA color message element.
        /// Checks the element type before reading & returns default if it's not interpretable as a color.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public string ReadStringElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            switch (Tags[index])
            {
                case TypeTag.AltTypeString:
                case TypeTag.String:
                    var length = 0;
                    while (m_SharedBuffer[offset + length] != byte.MinValue) length++;
                    return Encoding.ASCII.GetString(m_SharedBuffer, offset, length);
                case TypeTag.Float64:
                    m_SwapBuffer64[7] = m_SharedBuffer[offset];
                    m_SwapBuffer64[6] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer64[5] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer64[4] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer64[3] = m_SharedBuffer[offset + 4];
                    m_SwapBuffer64[2] = m_SharedBuffer[offset + 5];
                    m_SwapBuffer64[1] = m_SharedBuffer[offset + 6];
                    m_SwapBuffer64[0] = m_SharedBuffer[offset + 7];
                    double f64 = *SwapBuffer64Ptr;
                    return f64.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    float f32 = *SwapBuffer32Ptr;
                    return f32.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Int64:
                    var i64 = IPAddress.NetworkToHostOrder(m_SharedBuffer[offset]);
                    return i64.ToString(CultureInfo.CurrentCulture);
                case TypeTag.Int32:
                    int i32 = m_SharedBuffer[offset    ] << 24 |
                              m_SharedBuffer[offset + 1] << 16 |
                              m_SharedBuffer[offset + 2] <<  8 |
                              m_SharedBuffer[offset + 3];
                    return i32.ToString(CultureInfo.CurrentCulture);
                default:
                    return string.Empty;
            }
        }
        
        /// <summary>
        /// Read a single string message element, with NO TYPE SAFETY CHECK!
        /// Only call this if you are really sure that the element at the given index is a valid OSC string,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadStringElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            var length = 0;
            while (m_SharedBuffer[offset + length] != byte.MinValue) length++;
            return Encoding.ASCII.GetString(m_SharedBuffer, offset, length);
        }

        /// <summary>
        /// Read a single string message element as bytes.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">The byte array to copy the string's bytes to</param>
        /// <param name="copyOffset"></param>
        /// <returns>The byte length of the string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadStringElementBytes(int index, byte[] copyTo)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            
            int i;
            for (i = offset; i < m_SharedBuffer.Length; i++)
            {
                byte b = m_SharedBuffer[i];
                if (b == byte.MinValue) break;
                copyTo[i - offset] = b;
            }

            return i - offset;
        }
        
        /// <summary>
        /// Read a single string message element as bytes.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <param name="copyTo">The byte array to copy the string's bytes to</param>
        /// <param name="copyOffset">The index in the copyTo array to start copying at</param>
        /// <returns>The byte length of the string</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadStringElementBytes(int index, byte[] copyTo, int copyOffset)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            
            int i;
            // when this is subtracted from i, it's the same as i - offset + copyOffset
            var copyStartOffset = offset - copyOffset;
            for (i = offset; i < m_SharedBuffer.Length; i++)
            {
                byte b = m_SharedBuffer[i];
                if (b == byte.MinValue) break;
                copyTo[i - copyStartOffset] = b;
            }

            return i - offset;
        }
    }
}