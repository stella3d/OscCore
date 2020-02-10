using UnityEditor;

namespace OscCore
{
    [CustomEditor(typeof(OscSender))]
    public class OscSenderInspector : Editor
    {
        SerializedProperty m_IpAddressProp;
        SerializedProperty m_PortProp;

        void OnEnable()
        {
            m_IpAddressProp = serializedObject.FindProperty("m_IpAddress");
            m_PortProp = serializedObject.FindProperty("m_Port");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_IpAddressProp);
            EditorGUILayout.PropertyField(m_PortProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}


