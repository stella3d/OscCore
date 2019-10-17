using System;
using System.Collections.Generic;
using UnityEngine;

namespace OscCore
{
    [ExecuteAlways]
    public class OscServerRunner : MonoBehaviour
    {
        [SerializeField] int m_Port = 9000;

        OscServer m_Server;

        OscServer Server => m_Server;   
     
        void OnEnable()
        {
            if (m_Server == null)
            {
                m_Server = new OscServer(m_Port, 1024 * 16);
                m_Server.Start();
            }
            else
            {
                m_Server.Resume();
            }
        } 

        void OnDisable()
        {
            m_Server?.Pause();
        }

        void OnDestroy()
        {
            m_Server.Dispose();
        }
    }
}

