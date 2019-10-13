namespace OscCore
{
    public delegate void ReceiveValueMethod(OscMessageValues values);
    
    public delegate void MonitorCallback(string address, OscMessageValues values);
}