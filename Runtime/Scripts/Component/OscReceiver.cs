using UnityEngine;

namespace OscCore
{
    /// <summary>Wraps an OscServer in a Unity Component</summary>
    [ExecuteAlways]
    public class OscReceiver : MonoBehaviour
    {
        [Tooltip("The port to listen for incoming OSC messages on")]
        [SerializeField] int m_Port = 9000;

        OscServer m_Server;

        /// <summary>The underlying OscServer</summary>
        public OscServer Server => m_Server;   
     
        void OnEnable()
        {
            m_Server = OscServer.GetOrCreate(m_Port);
        }

        void Update()
        {
            m_Server.Update();
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

