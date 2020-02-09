using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Input/Boolean Message Handler")]
    public class OscBooleanMessageHandler : OscMessageHandler<bool, BoolUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadBooleanElement(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
