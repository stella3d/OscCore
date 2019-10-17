using System.Collections;
using System.Collections.Generic;
using OscCore;
using UnityEngine;

public class NetworkTimeClientRunner : MonoBehaviour
{
    NetworkTimeClient m_Client;
    
    void Awake()
    {
        m_Client = new NetworkTimeClient();
        m_Client.Start();
    }

    void Update()
    {
        m_Client.Update();
    }
}
