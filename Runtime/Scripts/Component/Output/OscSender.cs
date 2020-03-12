using UnityEngine;

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
                    string[] ipString = value.Split('.');

                    if(ipString.Length === 4){
                        m_IpAddress = value;
                        ReInit();
                    }
                }
        }

        /// <summary>The port on the remote IP to send to</summary>
        public int Port
        {
            get { return m_Port; }
            set { 
                    m_Port = value;
                    ReInit();
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

        /// <summary>
        /// Reinitializes the client. Use this if the IP or port change and the client needs to restart.
        /// </summary>
        public void ReInit()
        {
            Client = null;
            Setup();
        }
    }
}

