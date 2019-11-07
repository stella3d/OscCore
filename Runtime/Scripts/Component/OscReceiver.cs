using System;
using UnityEngine;

namespace OscCore
{
    [ExecuteAlways]
    public class OscReceiver : MonoBehaviour
    {
        [SerializeField] int m_Port = 9000;

        OscServer m_Server;

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

