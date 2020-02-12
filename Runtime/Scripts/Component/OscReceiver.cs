using UnityEngine;

namespace OscCore
{
    /// <summary>Wraps an OscServer in a Unity Component</summary>
    [AddComponentMenu("OSC/OSC Receiver", int.MinValue)]
    [ExecuteInEditMode]
    public class OscReceiver : MonoBehaviour
    {
        [Tooltip("The local port to listen for incoming messages on")]
        [SerializeField] int m_Port = 9000;

        /// <summary>The local post to listen to incoming messages on</summary>
        public int Port => m_Port;

        /// <summary>True if this receiver is bound to its port and listening, false otherwise</summary>
        public bool Running { get; private set; }

        /// <summary>The underlying server that handles message receiving.</summary>
        public OscServer Server { get; private set; }

        void OnEnable()
        {
            // OnEnable gets called twice when you enter play mode, but we just want one server instance
            if (Running) return;
            Server = OscServer.GetOrCreate(m_Port);
            Running = true;
        }

        void OnValidate()
        {
            m_Port = m_Port.ClampPort();
        }

        void Update()
        { 
            Server?.Update();
        }

        void OnDestroy()
        {
            Server?.Dispose();
        }

        void OnApplicationQuit()
        {
            Server?.Dispose();
        }
    }
}
