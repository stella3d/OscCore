namespace OscCore
{
    public class OscIntMessageHandler : OscMessageHandler<int>
    {
        public IntUnityEvent Handler;
        
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadIntElement(0);
        }
        
        protected override void InvokeEvent()
        {
            Handler.Invoke(m_Value);
        }
    }
}
