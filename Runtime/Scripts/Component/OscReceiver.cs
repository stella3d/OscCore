using UnityEngine;

namespace OscCore
{
    /// <summary>Wraps an OscServer in a Unity Component</summary>
    [AddComponentMenu("OSC/OSC Receiver", int.MinValue)]
    [ExecuteInEditMode]
    public class OscReceiver : MonoBehaviour
    {
        [Tooltip("The port to listen for incoming OSC messages on")]
        [SerializeField] int m_Port = 9000;

        OscServer m_Server;
        
        public bool Running { get; private set; }

        /// <summary>The underlying server that handles message receiving.  </summary>
        public OscServer Server => m_Server;
     
        void OnEnable()
        {
            // OnEnable gets called twice when you enter play mode, but we just want one server instance
            if (Running) return;
            m_Server = OscServer.GetOrCreate(m_Port);
            Running = true;
        }

        void OnValidate()
        {
            if (m_Port < 1024) m_Port = 1024;
            if (m_Port >= 65535) m_Port = 65535;
        }

        void Update()
        { 
            m_Server?.Update();
        }

        void OnDestroy()
        {
            m_Server?.Dispose();
        }

        void OnApplicationQuit()
        {
            m_Server?.Dispose();
        }
    }
}

