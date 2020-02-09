namespace OscCore
{
    public class OscInt64MessageHandler : OscMessageHandler<long, LongUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadInt64Element(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
