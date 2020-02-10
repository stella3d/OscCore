using System;
using System.Reflection;
using UnityEngine;

namespace OscCore
{
    [ExecuteInEditMode]
    public class PropertySender : MonoBehaviour
    {
#pragma warning disable 649
        //[Header("OSC Destination")]
        [SerializeField] OscSender m_Sender;
        
        [SerializeField] string m_Address = "";
        
        //[Header("Property Source")]
        [SerializeField] GameObject m_Object;
        
        [SerializeField] [HideInInspector]
        Component m_SourceComponent;
        
        [SerializeField] [HideInInspector] 
        string m_PropertyName;
        
        [SerializeField] [HideInInspector]
        string m_PropertyTypeName;
#pragma warning restore 649

        string[] m_PropertyList;
    
        Type m_PropertyType;

        int m_PreviousIntValue;
        float m_PreviousFloatValue;

        public PropertyInfo PropertyInfo { get; set; }

        void OnEnable()
        {
            if (m_Object == null) m_Object = gameObject;
        }

        void RefreshPropertyInfo()
        {
        }

        void Update()
        {
            if (string.IsNullOrEmpty(m_PropertyTypeName) || PropertyInfo == null) 
                return;
            if (m_Sender == null || m_Sender.Client == null) 
                return;
            
            var value = PropertyInfo.GetValue(m_SourceComponent);
            
            switch (m_PropertyTypeName)
            {
                case "Int32":
                    var intVal = (int) value;
                    if (intVal != m_PreviousIntValue)
                    {
                        m_PreviousIntValue = intVal;
                        m_Sender.Client.Send(m_Address, intVal);
                    }
                    break;
                case "Single":
                    var floatVal = (float) value;
                    if (floatVal != m_PreviousFloatValue)
                    {
                        m_PreviousFloatValue = floatVal;
                        m_Sender.Client.Send(m_Address, floatVal);
                    }
                    break;
                case "String":
                    m_Sender.Client.Send(m_Address, (string) value);
                    break;
                case "Color":
                    m_Sender.Client.Send(m_Address, (Color) value);
                    break;
                case "Vector3":
                    m_Sender.Client.Send(m_Address, (Vector3) value);
                    break;
            }
        }
        
        public Component[] GetObjectComponents()
        {
            return m_Object == null ? null : m_Object.GetComponents<Component>();
        }
    }
}

