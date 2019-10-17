using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OscCore
{
    public sealed class NetworkTimeClient
    {
        const string defaultNtpServer = "time.google.com";

        const int minUpdateInterval = 30;
        const int maxUpdateInterval = 60 * 10;
        const int defaultUpdateInterval = 60;
        
        readonly Socket m_Socket;
        
        readonly byte[] m_SendBuffer = new byte[48];
        readonly byte[] m_ReceiveArray = new byte[48];

        float m_UnscaledLastRequestTime;
        
        int m_ReceiveTimeoutMs = 6000;
        int m_SendTimeoutMs = 4000;
        
        int m_UpdateInterval = defaultUpdateInterval;
        
        Task m_ReceiveContinuationTask;

        IAsyncResult m_AsyncResult;
        bool m_RequestInProgress;
        int m_LastByteReadCount;
        
        /// <summary>The number of seconds between fetches of network time</summary>
        public int UpdateInterval 
        { 
            get => m_UpdateInterval;
            set
            {
                if (value > minUpdateInterval && value < maxUpdateInterval)
                    m_UpdateInterval = value;
            }
        }
        
        Stopwatch k_RequestTimer = new Stopwatch();
        
        public NtpTimestamp LastReceivedTime { get; private set; }

        public string NtpServer { get; private set; }

        public IPEndPoint ServerEndpoint { get; private set; }
        
        public NetworkTimeClient(string ntpServer = defaultNtpServer)
        {
            NtpServer = ntpServer;

            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = m_ReceiveTimeoutMs, SendTimeout = m_SendTimeoutMs
            };
            
            m_UnscaledLastRequestTime = Time.unscaledTime;
            m_SendBuffer[0] = 0x1B;
        }

        public void Start()
        {
            TryGetIp();
            m_Socket.Connect(ServerEndpoint);
            m_AsyncResult = SendTimeRequest();
        }

        public void Update()
        {
            if (!m_RequestInProgress)
            {
                if(Time.unscaledTime - m_UnscaledLastRequestTime > m_UpdateInterval)
                    m_AsyncResult = SendTimeRequest();
                
                return;
            }

            if (m_AsyncResult.IsCompleted)
            {
                if (m_LastByteReadCount == 48)
                {
                    LastReceivedTime = NtpTimestamp.FromBigEndianBytes(m_ReceiveArray, 40);
                    //Debug.Log("success!  " + LastReceivedTime);
                }
                
                m_RequestInProgress = false;
                m_AsyncResult = null;
            }
        }

        internal IAsyncResult SendTimeRequest()
        {
            try
            {
                m_UnscaledLastRequestTime = Time.unscaledTime;
                m_Socket.Send(m_SendBuffer);
                m_RequestInProgress = true;

                var completionSource = new TaskCompletionSource<int>(this);
                var reqTask = m_Socket.BeginReceive(m_ReceiveArray, 0, 48, SocketFlags.None, OnResponseReceived, completionSource);
                return reqTask;
            }
            catch (Exception e)
            {
                //Debug.LogException(e);
                throw;
            }
        }

        void OnResponseReceived(IAsyncResult result)
        {
            var asyncState = (TaskCompletionSource<int>) result.AsyncState;
            try
            {
                var sock = ((NetworkTimeClient)asyncState.Task.AsyncState).m_Socket;
                m_LastByteReadCount = sock.EndReceive(result);
                asyncState.SetResult(m_LastByteReadCount);
            }
            catch (Exception ex)
            {
                asyncState.TrySetException(ex);
            }
        }

        void TryGetIp()
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(NtpServer);
                var ipv4Addresses = hostEntry.AddressList.Where(
                    ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();

                if (ipv4Addresses.Length > 0)
                {
                    const int ntpPort = 123;
                    ServerEndpoint = new IPEndPoint(ipv4Addresses[0], ntpPort);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}