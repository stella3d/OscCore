using BlobHandles;

namespace OscCore
{
    public delegate void ReceiveValueMethod(OscMessageValues values);
    
    public delegate void MonitorCallback(BlobString address, OscMessageValues values);
}