namespace OscCore
{
    public class OscFloatMessageHandler : OscMessageHandler<float>
    {
        public FloatUnityEvent Handler;

        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadFloatElement(0);
        }
        
        protected override void InvokeEvent()
        {
            Handler.Invoke(m_Value);
        }
    }
}
