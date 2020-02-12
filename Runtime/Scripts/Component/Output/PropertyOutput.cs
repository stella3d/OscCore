using System;
using System.Reflection;
using UnityEngine;

namespace OscCore
{
    [ExecuteInEditMode]
    [AddComponentMenu("OSC/Property Output", int.MaxValue)]
    public class PropertyOutput : MonoBehaviour
    {
#pragma warning disable 649
        [Tooltip("Component that handles sending outgoing OSC messages")]
        [SerializeField] OscSender m_Sender;
        
        [Tooltip("The OSC address to send to at the destination")]
        [SerializeField] string m_Address = "";
        
        [Tooltip("The object host of the component where the property lives")]
        [SerializeField] GameObject m_Object;
        [SerializeField] Component m_SourceComponent;
        
        [SerializeField] string m_PropertyName;
        [SerializeField] string m_PropertyTypeName;
#pragma warning restore 649

        bool m_PreviousBooleanValue;
        long m_PreviousLongValue;
        double m_PreviousDoubleValue;
        string m_PreviousStringValue;
        Color m_PreviousColorValue;
        Vector2 m_PreviousVec2Value;
        Vector3 m_PreviousVec3Value;


        /// <summary>
        /// The Unity component that has the property to send.  Must be a type that has the current Property
        /// </summary>
        public Component SourceComponent
        {
            get => m_SourceComponent;
            set => m_SourceComponent = value;
        }

        /// <summary>
        /// The property to send the value of.  Must be a property found on the current SourceComponent
        /// </summary>
        public PropertyInfo Property { get; set; }

        void OnEnable()
        {
            if (m_Object == null) m_Object = gameObject;
        }

        void OnValidate()
        {
            Utils.ValidateAddress(ref m_Address);
            if (m_Sender == null) m_Sender = gameObject.GetComponentInParent<OscSender>();
        }

        void Update()
        {
            if (Property == null || m_Sender == null || m_Sender.Client == null) 
                return;
            
            var value = Property.GetValue(m_SourceComponent);
            
            switch (m_PropertyTypeName)
            {
                case "Int16":
                case "Int32":
                    if(ValueChanged(ref m_PreviousLongValue, value, out var intVal))
                        m_Sender.Client.Send(m_Address, intVal);
                    break;
                case "Int64":
                    if(ValueChanged(ref m_PreviousLongValue, value, out var longVal))
                        m_Sender.Client.Send(m_Address, longVal);
                    break;
                case "Single":
                    if(ValueChanged(ref m_PreviousDoubleValue, value, out var floatVal))
                        m_Sender.Client.Send(m_Address, floatVal);
                    break;
                case "Double":
                    if(ValueChanged(ref m_PreviousDoubleValue, value, out var doubleVal))
                        m_Sender.Client.Send(m_Address, doubleVal);
                    break;
                case "String":
                    if(ValueChanged(ref m_PreviousStringValue, value, out var stringVal))
                        m_Sender.Client.Send(m_Address, stringVal);
                    break;
                case "Color":
                case "Color32":
                    if(ValueChanged(ref m_PreviousColorValue, value, out var colorVal))
                        m_Sender.Client.Send(m_Address, colorVal);
                    break;
                case "Vector2":
                    if(ValueChanged(ref m_PreviousVec2Value, value, out var vec2Val))
                        m_Sender.Client.Send(m_Address, vec2Val);
                    break;
                case "Vector3":
                    if(ValueChanged(ref m_PreviousVec3Value, value, out var vec3Val))
                        m_Sender.Client.Send(m_Address, vec3Val);
                    break;
                case "Boolean":
                    if(ValueChanged(ref m_PreviousBooleanValue, value, out var boolVal))
                        m_Sender.Client.Send(m_Address, boolVal);
                    break;
            }
        }

        static bool ValueChanged<T>(ref T previousValue, object value, out T castValue) where T: IEquatable<T>
        {
            castValue = (T) value;
            if (!castValue.Equals(previousValue))
            {
                previousValue = castValue;
                return true;
            }

            return false;
        }

        internal Component[] GetObjectComponents()
        {
            return m_Object == null ? null : m_Object.GetComponents<Component>();
        }
    }
}

