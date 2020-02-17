using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace OscCore
{
    enum Vector3ElementFilter : byte
    {
        XYZ = 0,        // 0 instead of 7 (the combo of flags) so it defaults to this
        X = 1,
        Y = 2,
        Z = 3,
        XY = 4,
        XZ = 5,
        YZ = 6
    }
    
    enum Vector2ElementFilter : byte
    {
        XY = 0,        
        X = 1,
        Y = 2,
    }

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
        
        // controls which elements of a Vector3 are sent
        [SerializeField] 
        Vector3ElementFilter m_SendVector3Elements = Vector3ElementFilter.XYZ;
        [SerializeField] 
        Vector2ElementFilter m_SendVector2Elements = Vector2ElementFilter.XY;
#pragma warning restore 649

        bool m_PreviousBooleanValue;
        int m_PreviousIntValue;
        long m_PreviousLongValue;
        double m_PreviousDoubleValue;
        float m_PreviousSingleValue;
        string m_PreviousStringValue;
        Color m_PreviousColorValue;
        Vector2 m_PreviousVec2Value;
        Vector3 m_PreviousVec3Value;

        bool m_HasSender;
        
        /// <summary>
        /// The OscCore component that handles serializing and sending messages. Cannot be null
        /// </summary>
        public OscSender Sender
        {
            get => m_Sender;
            set
            {
                if(value != null) 
                    m_Sender = value;
            }
        }

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
            m_HasSender = m_Sender != null;
            SetPropertyFromSerialized();
        }

        void OnValidate()
        {
            Utils.ValidateAddress(ref m_Address);
            if (m_Sender == null) m_Sender = gameObject.GetComponentInParent<OscSender>();
            m_HasSender = m_Sender != null;
        }

        void Update()
        {
            if (Property == null || !m_HasSender || m_Sender.Client == null) 
                return;
            
            var value = Property.GetValue(m_SourceComponent);
            if (value == null)
                return;
            
            switch (m_PropertyTypeName)
            {
                case "Byte":
                case "SByte": 
                case "Int16":
                case "UInt16":    
                case "Int32":
                    if(ValueChanged(ref m_PreviousIntValue, value, out var intVal))
                        m_Sender.Client.Send(m_Address, intVal);
                    break;
                case "Int64":
                    if(ValueChanged(ref m_PreviousLongValue, value, out var longVal))
                        m_Sender.Client.Send(m_Address, longVal);
                    break;
                case "Single":
                    if(ValueChanged(ref m_PreviousSingleValue, value, out var floatVal))
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
                        SendVector2(vec2Val);
                    break;
                case "Vector3":
                    if (!ValueChanged(ref m_PreviousVec3Value, value, out var vec))
                        SendVector3(vec);
                    break;
                case "Boolean":
                    if(ValueChanged(ref m_PreviousBooleanValue, value, out var boolVal))
                        m_Sender.Client.Send(m_Address, boolVal);
                    break;
            }
        }
        
        void SendVector2(Vector2 vec)
        {
            switch (m_SendVector2Elements)
            {
                case Vector2ElementFilter.XY:
                    m_Sender.Client.Send(m_Address, vec);
                    break;
                case Vector2ElementFilter.X:
                    m_Sender.Client.Send(m_Address, vec.x);
                    break;
                case Vector2ElementFilter.Y:
                    m_Sender.Client.Send(m_Address, vec.y);
                    break;
            }
        }

        void SendVector3(Vector3 vec)
        {
            switch (m_SendVector3Elements)
            {
                case Vector3ElementFilter.XYZ:
                    m_Sender.Client.Send(m_Address, vec);
                    break;
                case Vector3ElementFilter.X:
                    m_Sender.Client.Send(m_Address, vec.x);
                    break;
                case Vector3ElementFilter.Y:
                    m_Sender.Client.Send(m_Address, vec.y);
                    break;
                case Vector3ElementFilter.Z:
                    m_Sender.Client.Send(m_Address, vec.z);
                    break;
                case Vector3ElementFilter.XY:
                    m_Sender.Client.Send(m_Address, new Vector2(vec.x, vec.y));
                    break;
                case Vector3ElementFilter.XZ:
                    m_Sender.Client.Send(m_Address, new Vector2(vec.x, vec.z));
                    break;
                case Vector3ElementFilter.YZ:
                    m_Sender.Client.Send(m_Address, new Vector2(vec.y, vec.z));
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

        internal void SetPropertyFromSerialized()
        {
            if (m_SourceComponent == null) 
                return;
            
            var type = m_SourceComponent.GetType();
            Property = type.GetProperty(m_PropertyName);
        }
    }
}
