using UnityEditor;
using UnityEngine;

namespace OscCore
{
    [CustomEditor(typeof(OscReceiver))]
    public class OscReceiverInspector : Editor
    {
        OscReceiver m_Target;
        SerializedProperty m_PortProp;
       
        void OnEnable()
        {
            m_Target = (OscReceiver) target;
            m_PortProp = serializedObject.FindProperty("m_Port");
        }

        public override void OnInspectorGUI()
        {
            var running = m_Target != null && m_Target.Running;
            
            EditorGUI.BeginDisabledGroup(running && Application.IsPlaying(this));
            EditorGUILayout.PropertyField(m_PortProp);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}


