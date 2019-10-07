using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed partial class OscMessageValues
    {
        /// <summary>
        /// Read a non-standard
        /// Checks the element type before reading & returns default if it's not interpretable as a color.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBooleanElement(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogError($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            switch (Tags[index])
            {
                case TypeTag.True: return true;
                case TypeTag.False: return false;
                default: return default;
            }
        }
    }
}