using System;
using UnityEngine;

namespace OscCore
{
    [AddComponentMenu("OSC/OSC Sender", int.MaxValue - 10)]
    public class OscSender : MonoBehaviour
    {
        [Tooltip("The IP address to send to")]
        [SerializeField] string m_IpAddress = "127.0.0.1";

        [Tooltip("The port to send to")]
        [SerializeField] int m_Port = 7000;
        
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
    }
}

