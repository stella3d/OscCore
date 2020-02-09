using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Message Handler/Integer")]
    public class OscIntMessageHandler : OscMessageHandler<int, IntUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadIntElement(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
