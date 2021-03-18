using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OscCore.Tests
{
    [Serializable]
    [CreateAssetMenu(fileName="NewOscSequence", menuName="OSC/Message Sequence", order=100)]
    public class OscMessageSequence : ScriptableObject
    {
        public TimedMessage[] Messages;

        public OscMessageSequence (string name, TimedMessage[] messages)
        {
            this.name = name != null ? name : "New OSC Sequence";
            this.Messages = messages;
        }

        public static OscMessageSequence FromJson(string json)
        {
            var parsed = JsonUtility.FromJson<MiniMessageSequence>(json);
            return new OscMessageSequence(parsed.name, parsed.messages.Select(m => m.ToFriendly()).ToArray());
        }

        public string ToJson(bool pretty = false) => JsonUtility.ToJson(this, pretty);
    }

    [Serializable]
    public class TimedMessage
    {
        public float Time;
        public OscMessage Message;

        public TimedMessage(float time, OscMessage message)
        {
            Time = time;
            Message = message;
        }
    }

    [Serializable]
    internal class MiniMessageSequence
    {
        public string name;
        public MiniTimedMessage[] messages;

        public MiniMessageSequence (OscMessageSequence seq)
        {
            this.name = seq.name;
            this.messages = seq.Messages.Select(m => new MiniTimedMessage(m)).ToArray();
        }
    }

    [Serializable]
    internal class MiniTimedMessage
    {
        public float t;
        public string addr;
        public string tags;
        public string data;

        public TimedMessage ToFriendly()
        {
            return new TimedMessage(t, new OscMessage(addr, tags, Encoding.ASCII.GetBytes(data)));
        }

        public MiniTimedMessage (TimedMessage tm)
        {
            this.t = tm.Time;

            var msg = tm.Message;
            this.addr = msg.Address;
            this.tags = msg.TypeTags;
            this.data = Encoding.ASCII.GetString(msg.Data);
        }
    }
}
