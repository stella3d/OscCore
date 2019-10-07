using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit float message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a float.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public float ReadFloatElement(int index)
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
                case TypeTag.Float32:
                    m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
                    m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
                    m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
                    m_SwapBuffer32[3] = m_SharedBuffer[offset];
                    return *SwapBuffer32Ptr;
                case TypeTag.Int32:
                    return m_SharedBuffer[index    ] << 24 | 
                           m_SharedBuffer[index + 1] << 16 |
                           m_SharedBuffer[index + 2] <<  8 |
                           m_SharedBuffer[index + 3];
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single 32-bit float message element, with NO TYPE SAFETY CHECK!
        /// Only call this if you are really sure that the element at the given index is a valid float,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            var offset = Offsets[index];
            m_SwapBuffer32[0] = m_SharedBuffer[offset + 3];
            m_SwapBuffer32[1] = m_SharedBuffer[offset + 2];
            m_SwapBuffer32[2] = m_SharedBuffer[offset + 1];
            m_SwapBuffer32[3] = m_SharedBuffer[offset];
            return *SwapBuffer32Ptr;
        }
    }
}