namespace OscCore
{
    public struct MidiMessage
    {
        public byte PortId { get; private set; }
        public byte Status { get; private set; }
        public byte Data1 { get; private set; }
        public byte Data2 { get; private set; }

        public MidiMessage(byte[] bytes, int offset)
        {
            PortId = bytes[offset];
            Status = bytes[offset + 1];
            Data1 = bytes[offset + 2];
            Data2 = bytes[offset + 3];
        }

        public void SetBytes(byte[] bytes, int offset)
        {
            PortId = bytes[offset];
            Status = bytes[offset + 1];
            Data1 = bytes[offset + 2];
            Data2 = bytes[offset + 3];
        }

        public override string ToString()
        {
            return $"Port ID: {PortId}, Status: {Status}, Data: {Data1} , {Data2}";
        }
    }
}

