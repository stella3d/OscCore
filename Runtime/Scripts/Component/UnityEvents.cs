using System;
using UnityEngine.Events;

namespace OscCore
{
    [Serializable] public class IntUnityEvent : UnityEvent<int> { }
    [Serializable] public class FloatUnityEvent : UnityEvent<float> { }
    [Serializable] public class StringUnityEvent : UnityEvent<string> { }
}