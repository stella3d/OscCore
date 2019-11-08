namespace OscCore
{
    public class OscInt64MessageHandler : OscMessageHandler<long>
    {
        public LongUnityEvent Handler;
        
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadInt64Element(0);
        }
        
        protected override void InvokeEvent()
        {
            Handler.Invoke(m_Value);
        }
    }
}
