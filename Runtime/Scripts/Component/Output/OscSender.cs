using UnityEngine;

namespace OscCore
{
    public class OscSender : MonoBehaviour
    {
        [SerializeField] string m_IpAddress = "127.0.0.1";
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

        void Setup()
        {
            if(Client == null)
                Client = new OscClient(m_IpAddress, m_Port);
        }
    }
}

