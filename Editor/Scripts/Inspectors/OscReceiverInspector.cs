using UnityEditor;

namespace OscCore
{
    [CustomEditor(typeof(OscReceiver))]
    public class OscReceiverInspector : Editor
    {
        SerializedProperty m_PortProp;
       
        void OnEnable()
        {
            m_PortProp = serializedObject.FindProperty("m_Port");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_PortProp);
        }
    }
}


