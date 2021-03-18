using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OscCore.Tests
{
    public class MessageSequenceGenerator : MonoBehaviour
    {
        [Range(2, 1024)]
        public int Length = 32;

        public bool MultiElementMessages;
        public bool NonStandardTypes;
        public bool Bundled;

        public void Start()
        {
            var seq = OscRandom.GetSequence(Length, MultiElementMessages, NonStandardTypes, Bundled);

            foreach (var msg in seq.Messages)
                Debug.Log(msg);
        }
    }
}