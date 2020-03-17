using UnityEngine;
using System.Net;

namespace OscCore
{
    [ExecuteInEditMode]
    [AddComponentMenu("OSC/OSC Sender", int.MaxValue - 10)]
    public class OscSender : MonoBehaviour
    {
        [Tooltip("The IP address to send to")]
        [SerializeField] string m_IpAddress = "127.0.0.1";
        
        [Tooltip("The port on the remote IP to send to")]
        [SerializeField] int m_Port = 7000;

        /// <summary>The IP address to send to</summary>
        public string IpAddress
        {
            get { return m_IpAddress; }
            set {
                    if(IPAddress.TryParse(value, out var ip)){
                        m_IpAddress = value;
                        ReInitialize();
                    }
                }
        }

        /// <summary>The port on the remote IP to send to</summary>
        public int Port
        {
            get { return m_Port; }
            set { 
                    m_Port = value;
                    ReInitialize();
                }
        }
        
        /// <summary>
        /// Handles serializing and sending messages.  Use methods on this to send messages to the endpoint.
        /// </summary>
        public OscClient Client { get; protected set; }
        
        void OnEnable()
        {
            Setup();
        }

        void Awake()
        {
            Setup();
        }
        
        void OnValidate()
        {
            m_Port = m_Port.ClampPort();
        }

        void Setup()
        {
            if(Client == null)
                Client = new OscClient(m_IpAddress, m_Port);
        }

        void ReInitialize()
        {
            Client = null;
            Setup();
        }
    }
}

