using UnityEditor;

namespace OscCore
{
    [CustomEditor(typeof(OscMessageHandler<,>), true)]
    public class OscMessageHandlerInspector : Editor
    {
        SerializedProperty m_ReceiverProp;
        SerializedProperty m_AddressProp;
        SerializedProperty m_HandlerProp;
       
        void OnEnable()
        {
            m_ReceiverProp = serializedObject.FindProperty("m_Receiver");
            m_AddressProp = serializedObject.FindProperty("m_Address");
            m_HandlerProp = serializedObject.FindProperty("OnMessageReceived");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_ReceiverProp);
            EditorGUILayout.PropertyField(m_AddressProp);
            EditorGUILayout.Space();
            if (m_HandlerProp != null)
                EditorGUILayout.PropertyField(m_HandlerProp);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}


