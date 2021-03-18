using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OscCore.Tests
{
    public class MessageSequenceSender
    {
        public string Name;

        public List<TimedMessage> Messages = new List<TimedMessage>();

        public static OscMessageSequence FromJson(string json)
        {
            var parsed = JsonUtility.FromJson<MiniMessageSequence>(json);

            return new OscMessageSequence()
            {
                Name = parsed.name,
                Messages = parsed.messages.Select(m => m.ToFriendly()).ToList()
            };
        }
    }
}