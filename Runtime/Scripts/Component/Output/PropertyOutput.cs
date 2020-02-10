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

        public PropertyInfo PropertyInfo { get; set; }

        void OnEnable()
        {
            if (m_Object == null) m_Object = gameObject;
        }

        void OnValidate()
        {
            m_Address = Utils.ValidateAddress(m_Address);
        }

        void Update()
        {
            if (PropertyInfo == null || m_Sender == null || m_Sender.Client == null) 
                return;
            
            var value = PropertyInfo.GetValue(m_SourceComponent);
            
            switch (m_PropertyTypeName)
            {
                case "Int32":
                    var intVal = (int) value;
                    if (intVal != m_PreviousLongValue)
                    {
                        m_PreviousLongValue = intVal;
                        m_Sender.Client.Send(m_Address, intVal);
                    }
                    break;
                case "Int64":
                    var longVal = (long) value;
                    if (longVal != m_PreviousLongValue)
                    {
                        m_PreviousLongValue = longVal;
                        m_Sender.Client.Send(m_Address, longVal);
                    }
                    break;
                case "Single":
                    var floatVal = (float) value;
                    if (floatVal != m_PreviousDoubleValue)
                    {
                        m_PreviousDoubleValue = floatVal;
                        m_Sender.Client.Send(m_Address, floatVal);
                    }
                    break;
                case "Double":
                    var doubleVal = (double) value;
                    if (doubleVal != m_PreviousDoubleValue)
                    {
                        m_PreviousDoubleValue = doubleVal;
                        m_Sender.Client.Send(m_Address, doubleVal);
                    }
                    break;
                case "String":
                    var stringVal = (string) value;
                    if (stringVal != m_PreviousStringValue)
                    {
                        m_PreviousStringValue = stringVal;
                        m_Sender.Client.Send(m_Address, stringVal);
                    }
                    break;
                case "Color":
                case "Color32":
                    var colorVal = (Color) value;
                    if (colorVal != m_PreviousColorValue)
                    {
                        m_PreviousColorValue = colorVal;
                        m_Sender.Client.Send(m_Address, colorVal);
                    }
                    break;
                case "Vector2":
                    var vec2Val = (Vector2) value;
                    if (vec2Val != m_PreviousVec2Value)
                    {
                        m_PreviousVec2Value = vec2Val;
                        m_Sender.Client.Send(m_Address, vec2Val);
                    }
                    break;
                case "Vector3":
                    var vec3Val = (Vector3) value;
                    if (vec3Val != m_PreviousVec3Value)
                    {
                        m_PreviousVec3Value = vec3Val;
                        m_Sender.Client.Send(m_Address, vec3Val);
                    }
                    break;
                case "Boolean":
                    var boolVal = (bool) value;
                    if (boolVal != m_PreviousBooleanValue)
                    {
                        m_PreviousBooleanValue = boolVal;
                        m_Sender.Client.Send(m_Address, boolVal);
                    }
                    break;
            }
        }

        public Component[] GetObjectComponents()
        {
            return m_Object == null ? null : m_Object.GetComponents<Component>();
        }
    }
}

