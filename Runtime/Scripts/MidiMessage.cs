using System;
using System.Runtime.InteropServices;

namespace OscCore
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiMessage : IEquatable<MidiMessage>
    {
        public readonly byte PortId;
        public readonly byte Status;
        public readonly byte Data1;
        public readonly byte Data2;

        public MidiMessage(byte[] bytes, int offset)
        {
            PortId = bytes[offset];
            Status = bytes[offset + 1];
            Data1 = bytes[offset + 2];
            Data2 = bytes[offset + 3];
        }
        
        public MidiMessage(byte portId, byte status, byte data1, byte data2)
        {
            PortId = portId;
            Status = status;
            Data1 = data1;
            Data2 = data2;
        }

        public override string ToString()
        {
            return $"Port ID: {PortId}, Status: {Status}, Data: {Data1} , {Data2}";
        }

        public bool Equals(MidiMessage other)
        {
            return PortId == other.PortId && Status == other.Status && Data1 == other.Data1 && Data2 == other.Data2;
        }

        public override bool Equals(object obj)
        {
            return obj is MidiMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Status.GetHashCode();
                hashCode = (hashCode * 397) ^ Data1.GetHashCode();
                hashCode = (hashCode * 397) ^ Data2.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MidiMessage left, MidiMessage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MidiMessage left, MidiMessage right)
        {
            return !left.Equals(right);
        }
    }
}

