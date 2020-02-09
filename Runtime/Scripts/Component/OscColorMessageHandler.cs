using UnityEngine;

namespace OscCore
{
    public class OscColorMessageHandler : OscMessageHandler<Color, ColorUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadColor32Element(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
