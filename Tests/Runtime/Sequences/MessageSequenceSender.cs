using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OscCore.Tests
{
    public class MessageSequenceSender : MonoBehaviour
    {
        public OscMessageSequence Sequence;

        public OscSender Sender;

        int m_LastSentIndex = 0;
        TimedMessage m_NextMessage;

        public void Start()
        {
            if(Sequence == null) 
            {
                enabled = false;
                return;
            }

            m_NextMessage = Sequence.Messages[0];
        }

        public void Update()
        {
            var t = Time.time;
            if(t >= m_NextMessage.Time)
            {
                var msg = m_NextMessage.Message;
                //Sender.Client.Send(msg.Address, msg.TypeTags, msg.Data);
            }
        }
    }
}