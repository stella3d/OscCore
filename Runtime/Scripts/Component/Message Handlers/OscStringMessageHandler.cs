using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/String Message Handler")]
    public class OscStringMessageHandler : OscMessageHandler<string, StringUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadStringElement(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
