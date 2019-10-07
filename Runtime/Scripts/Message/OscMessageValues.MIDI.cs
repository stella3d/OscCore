using System.Runtime.CompilerServices;

namespace OscCore
{
    public sealed unsafe partial class OscMessageValues
    {
        /// <summary>
        /// Read a single 32-bit integer message element.
        /// Checks the element type before reading & returns 0 if it's not interpretable as a integer.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        public MidiMessage ReadMidiElement(int index)
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
                case TypeTag.MIDI:
                    return *(MidiMessage*) (SharedBufferPtr + offset);
                default:
                    return default;
            }
        }
        
        /// <summary>
        /// Read a single MIDI message element, with NO TYPE SAFETY CHECK!
        /// Only call this if you are really sure that the element at the given index is a valid MIDI message,
        /// as the performance difference is small.
        /// </summary>
        /// <param name="index">The element index</param>
        /// <returns>The value of the element</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MidiMessage ReadMidiElementUnchecked(int index)
        {
#if OSCCORE_SAFETY_CHECKS
            if (index >= ElementCount)
            {
                Debug.LogWarning($"Tried to read message element index {index}, but there are only {ElementCount} elements");
                return default;
            }
#endif
            return *(MidiMessage*) (SharedBufferPtr + Offsets[index]);
        }
    }
}