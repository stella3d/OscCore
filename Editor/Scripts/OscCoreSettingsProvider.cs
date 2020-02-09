using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OscCore
{
    static class EditorHelp
    {
        public const string PrefKey = "OscCore_ShowEditorHelp";

        public static bool Show => EditorPrefs.GetBool(PrefKey, true);
    }

    static class OscCoreSettingsIMGUIRegister
    {
        
        const string k_HelpTooltip = "If enabled, display tutorial & hint messages in the Editor";
        static readonly GUIContent k_HelpContent = new GUIContent("Show Help", k_HelpTooltip);
        
        [SettingsProvider]
        public static SettingsProvider CreateOscCoreSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider("User/Open Sound Control Core", SettingsScope.User)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = "OSC Core",
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    if (EditorPrefs.HasKey(EditorHelp.PrefKey))
                    {
                        var setting = EditorPrefs.GetBool(EditorHelp.PrefKey);
                        var afterSetting = EditorGUILayout.Toggle(k_HelpContent, setting);
                        if(afterSetting != setting)
                            EditorPrefs.SetBool(EditorHelp.PrefKey, afterSetting);
                    }
                    else
                    {
                        EditorPrefs.SetBool(EditorHelp.PrefKey, true);
                    }
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "OSC", "Help" , "Open Sound" })
            };

            return provider;
        }
    }
}


