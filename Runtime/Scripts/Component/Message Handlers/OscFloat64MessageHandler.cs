﻿namespace OscCore
{
    public class OscFloat64MessageHandler : OscMessageHandler<double, DoubleUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadFloat64Element(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}