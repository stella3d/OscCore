using UnityEngine;

namespace OscCore
{
    public class OscFloatMessageHandler : OscMessageHandler<float, FloatUnityEvent>
    {
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
