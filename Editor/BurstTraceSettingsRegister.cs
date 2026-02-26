using Elfinik.BurstTrace.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;


namespace Elfinik.BurstTrace.EditorScripts
{
    public class BurstTraceSettingsRegister
    {
        private static readonly string[] MyDefines = { "BURSTTRACE_NOT_USE_64", "BURSTTRACE_CAPTURE_PROFILER", "BURSTTRACE_DISABLE", "BURSTTRACE_FREE_MEMORY", "BURSTTRACE_OPTIMIZE_MEMORY" };
        private static BurstTraceConfig _settings;

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/BurstTrace", SettingsScope.Project)
            {
                label = "BurstTrace",
                guiHandler = (searchContext) =>
                {
                    if (_settings == null) BurstTraceConfig.TryGetSettings(out _settings);

                    DrawCopyableHelpBox("If needed, you can copy any text from the tooltip and paste it into a translator!", MessageType.Info);
                    EditorGUILayout.LabelField("Global Config (Recompilation is required)", EditorStyles.boldLabel);
                    EditorGUILayout.Space();

                    BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                    string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                    List<string> definesList = currentDefines.Split(';').ToList();


                    EditorGUI.BeginChangeCheck();

                    bool toggleE = DrawDefineToggle("Memory optimization mode (~x3)", MyDefines[4], definesList, $"It optimizes storage memory usage by ~3 times. In return, it creates minor overhead during the initial log registration and when reading logs, and may also break logging (output string) for projects with deep internal (within Assets) nesting. It is not recommended to enable this option if the local path (within Assets) of your files exceeds 124 bytes!");
#if BURSTTRACE_NOT_USE_64
                    if(toggleE)
                        EditorGUILayout.HelpBox($"This function does not work when the \"Disable 64-hash optimization\" option is enabled.", MessageType.Error);
#endif
                    bool toggleA = DrawDefineToggle("Disable 64-hash optimization", MyDefines[0], definesList, $"Enable this only if you encounter a hash collision in the logs. However, the chance of this happening is incredibly small. This setting will slightly reduce optimization, but it guarantees the absence of hash collisions.");
                    bool toggleB = DrawDefineToggle("Capture profiler", MyDefines[1], definesList, $"Enable this option so that every logging event is recorded in the profiler: this will allow you to see the actual load on the system. \r\n(Don't forget to disable it in the release version!)");
                    bool toggleC = DrawDefineToggle("Disable logs", MyDefines[2], definesList, $"Completely disable logging. Any calculations performed when calling logging functions will be removed, reducing overhead to zero. However, please note: <b>TraceHandle</b> still occupies 4 bytes, even though it is empty. This is done for safety and to prevent accidental failures during serialization, ensuring that the size does not change between builds.");
                    //bool toggleD = DrawDefineToggle("Free memory", MyDefines[3], definesList, $"If you are absolutely sure that you will not have any problems with resizing the structure, check this box: it will make the <b>TraceHandle</b> structure empty. This can save memory in ECS, but it is not recommended for stability reasons.\r\nIgnored if logging is enabled");


                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdateDefines(targetGroup, definesList);
                    }


                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("Memory configuration", EditorStyles.boldLabel);



                    if (_settings == null)
                    {
                        EditorGUILayout.HelpBox($"Config profile not created", MessageType.Error);
                        if (GUILayout.Button("Create config file", GUILayout.Height(30)))
                        {
                            CreateSettingsAsset();
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.BeginHorizontal();

                        int rawValue = EditorGUILayout.IntSlider("Alloc Size", _settings.preallocRows, 64, 4096);
                        _settings.preallocRows = Mathf.RoundToInt(rawValue / 64f) * 64;

#if !BURSTTRACE_NOT_USE_64
                        float mbValue = _settings.preallocRows * (40 + UnsafeUtility.SizeOf<DetailedLog>()) / 1024F / 1024f;
#else
                        float mbValue = _settings.preallocRows * (40 + 512) / 1024F/ 1024f ;
#endif
                        mbValue *= JobsUtility.ThreadIndexCount;
                        GUI.enabled = false;
                        EditorGUILayout.TextField($"~{mbValue:F1} MB", GUILayout.Width(60));
                        GUI.enabled = true;

                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(_settings);
                            AssetDatabase.SaveAssets();
                        }
                        DrawCopyableHelpBox("Specify the amount of memory allocated for the log when the application is initialized. Leave reasonable values for desktop platforms. You can reduce the value for mobile platforms. The number here represents the number of UNIQUE log entries that are created frequently. If logs are created infrequently, there may be up to 20 times more entries than the specified number. See the documentation for more details.", MessageType.Info);
                    }
                },


                keywords = new HashSet<string>(new[] { "BurstTrace", "Memory", "Alloc", "Debug", "Log" })
            };

            return provider;
        }
        private static void CreateSettingsAsset()
        {
            string directory = "Assets/Resources";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            _settings = ScriptableObject.CreateInstance<BurstTraceConfig>();
            AssetDatabase.CreateAsset(_settings, $"{directory}/{BurstTraceConfig.FILE_PATH}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private static bool DrawDefineToggle(string label, string define, List<string> currentDefines, string tooltip)
        {
            bool isActive = currentDefines.Contains(define);
            bool newState = EditorGUILayout.ToggleLeft(label, isActive);

            DrawCopyableHelpBox(tooltip, MessageType.Info);

            if (newState && !isActive) currentDefines.Add(define);
            else if (!newState && isActive) currentDefines.Remove(define);

            return newState;
        }

        private static void UpdateDefines(BuildTargetGroup group, List<string> definesList)
        {
            string newDefines = string.Join(";", definesList.Distinct());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, newDefines);
        }
        private static void DrawCopyableHelpBox(string text, MessageType type)
        {
            string iconName = type switch
            {
                MessageType.Info => "console.infoicon",
                MessageType.Warning => "console.warnicon",
                MessageType.Error => "console.erroricon",
                _ => ""
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(iconName))
            {
                var iconContent = EditorGUIUtility.IconContent(iconName);
                GUILayout.Label(iconContent, GUILayout.Width(25), GUILayout.Height(25));
            }

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.label);
            textAreaStyle.wordWrap = true;
            textAreaStyle.richText = true;

            EditorStyles.label.wordWrap = true;

            EditorGUILayout.TextArea(
                text,
                textAreaStyle,
                GUILayout.ExpandHeight(false)
            );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}