using UnityEngine;

namespace OscCore
{
    /// <summary>Wraps an OscServer in a Unity Component</summary>
    [ExecuteAlways]
    public class OscReceiver : MonoBehaviour
    {
        [Tooltip("The port to listen for incoming OSC messages on")]
        [SerializeField] int m_Port = 9000;

        bool m_Started;
        OscServer m_Server;

        /// <summary>The underlying server that handles message receiving.  </summary>
        public OscServer Server => m_Server;
     
        void OnEnable()
        {
            // OnEnable gets called twice when you enter play mode, but we just want one server instance
            if (m_Started) return;
            m_Server = OscServer.GetOrCreate(m_Port);
            m_Started = true;
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

