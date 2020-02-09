using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/Message Handler/Float")]
    public class OscFloatMessageHandler : OscMessageHandler<float, FloatUnityEvent>
    {
        protected override void ValueRead(OscMessageValues values)
        {
            m_Value = values.ReadFloatElement(0);
        }
        
        protected override void InvokeEvent()
        {
            OnMessageReceived.Invoke(m_Value);
        }
    }
}
