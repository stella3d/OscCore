using UnityEngine;

namespace OscCore
{
    public abstract class OscMessageHandler<T> : MonoBehaviour
    {
        public OscReceiver Receiver;
    
        public string Address;
    
        protected T m_Value;
        protected OscActionPair m_ActionPair;
        protected bool m_Registered;    
        
        void OnEnable()
        {
            if (m_Registered || string.IsNullOrEmpty(Address))
                return;
        
            if (Receiver == null)
                Receiver = GetComponentInParent<OscReceiver>();

            if (Receiver != null && Receiver.Server != null)
            {
                m_ActionPair = new OscActionPair(ValueRead, InvokeEvent);
                Receiver.Server.TryAddMethodPair(Address, m_ActionPair);
                m_Registered = true;
            }
        }

        void OnDisable()
        {
            m_Registered = false;
            if (Receiver != null)
                Receiver.Server.RemoveMethodPair(Address, m_ActionPair);
        }

        protected abstract void InvokeEvent();
    
        protected abstract void ValueRead(OscMessageValues values);

        // Empty update method here so the component gets an enable checkbox
        protected virtual void Update() { }
    }
}

