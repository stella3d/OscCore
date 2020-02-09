namespace OscCore
{
    public class OscStringMessageHandler : OscMessageHandler<string, StringUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadStringElement(0);
        }
        
        protected override void InvokeEvent()
        {
            Handler.Invoke(m_Value);
        }
    }
}
