namespace OscCore
{
    static class EditorHelp
    {
        public const string PrefKey = "OscCore_ShowEditorHelp";

        public static bool Show => UnityEditor.EditorPrefs.GetBool(PrefKey, true);
    }
}


