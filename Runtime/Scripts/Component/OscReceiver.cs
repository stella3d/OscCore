using System;
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

        // TODO - add setter here and support port switching
        /// <summary>The local port to listen to incoming messages on</summary>
        public int Port
        {
            get => m_Port;
            set => SetPort(value);
        }

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

        void SetPort(int newPort)
        {
            var clamped = newPort.ClampPort();
            if (clamped != newPort) return;

            var oldValue = m_Port;
            var oldServer = Server;
            try
            {
                Server = OscServer.GetOrCreate(newPort);
                m_Port = newPort;
                oldServer?.Dispose();
            }
            // if creating a new server throws for any reason, make sure to keep old settings
            catch (Exception e)
            {
                m_Port = oldValue;
                Server = oldServer;
            }
        }
    }
}
