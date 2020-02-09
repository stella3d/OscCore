using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace OscCore
{
    [ExecuteInEditMode]
    public abstract class OscMessageHandler<T, TUnityEvent> : MonoBehaviour
        where TUnityEvent : UnityEvent<T>
    {
        [Tooltip("The receiver to handle messages from")]
        [FormerlySerializedAs("Receiver")]
        [SerializeField] 
        protected OscReceiver m_Receiver;
        public OscReceiver Receiver => m_Receiver;
    
        // TODO - add check for this address being valid in inspector ?
        [Tooltip("The OSC address to associate with this event.  Must start with /")]
        [FormerlySerializedAs("Address")]
        [SerializeField] 
        protected string m_Address;
        public string Address => m_Address;
    
        [FormerlySerializedAs("Handler")]
        public TUnityEvent OnMessageReceived;
        
        protected T m_Value;
        protected OscActionPair m_ActionPair;
        protected bool m_Registered;
        
        void OnEnable()
        {
            if (m_Receiver == null)
                m_Receiver = GetComponentInParent<OscReceiver>();
            
            if (m_Registered || string.IsNullOrEmpty(Address))
                return;

            if (m_Receiver != null && m_Receiver.Server != null)
            {
                m_ActionPair = new OscActionPair(ValueRead, InvokeEvent);
                Receiver.Server.TryAddMethodPair(Address, m_ActionPair);
                m_Registered = true;
            }
        }

        void OnDisable()
        {
            m_Registered = false;
            if (m_Receiver != null)
                m_Receiver.Server?.RemoveMethodPair(Address, m_ActionPair);
        }

        protected abstract void InvokeEvent();
    
        protected abstract void ValueRead(OscMessageValues values);

        // Empty update method here so the component gets an enable checkbox
        protected virtual void Update() { }
    }
}

