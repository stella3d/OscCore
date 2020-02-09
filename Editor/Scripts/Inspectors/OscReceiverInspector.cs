using UnityEditor;
using UnityEngine;

namespace OscCore
{
    [CustomEditor(typeof(OscReceiver))]
    public class OscReceiverInspector : Editor
    {
        OscReceiver m_Target;
        SerializedProperty m_PortProp;
        
        static readonly GUIContent k_CountContent = new GUIContent("Address Count",
                "The number of unique OSC Addresses registered on this port");

        const string k_HelpText = "Handles receiving & parsing OSC messages on the given port.\n" +
                                  "Forwards messages to all event handler components that reference it.";

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

            EditorGUI.BeginDisabledGroup(true);
            var numContent = new GUIContent(CountHandlers().ToString());
            EditorGUILayout.LabelField(k_CountContent, numContent, EditorStyles.boldLabel);
            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
            
            if (EditorHelp.Show)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(k_HelpText, MessageType.Info);
            }
        }

        int CountHandlers()
        {
            return m_Target == null || m_Target.Server == null ? 0 : m_Target.Server.CountHandlers();
        }
    }
}


