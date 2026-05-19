#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Elfinik.BurstTrace.ReadMe
{
    [CustomEditor(typeof(BurstTraceReadme))]
    public class BurstTraceReadmeEditor : Editor
    {
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle codeStyle;
        private GUIStyle helpBoxStyle;

        private void InitStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };

                textStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { fontSize = 16 };

                codeStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 5, 5)
                };

                helpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 15,
                    wordWrap = true,
                    padding = new RectOffset(10, 10, 10, 10)
                };
            }
        }

        public override void OnInspectorGUI()
        {
            bool previousGuiState = GUI.enabled;

            GUI.enabled = true;

            InitStyles();

            GUILayout.Space(10);
            GUILayout.Label("Documentation & Links", titleStyle);
            GUILayout.Space(5);

            GUILayout.Box("Select a language below to open the online GitHub documentation.", helpBoxStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("EN", GUILayout.Height(25))) Application.OpenURL("https://github.com/Elfinik/burst-trace/blob/main/README.md");
            if (GUILayout.Button("JP", GUILayout.Height(25))) Application.OpenURL("https://github.com/Elfinik/burst-trace/blob/main/README-JP.md");
            if (GUILayout.Button("CN", GUILayout.Height(25))) Application.OpenURL("https://github.com/Elfinik/burst-trace/blob/main/README-CN.md");
            if (GUILayout.Button("RU", GUILayout.Height(25))) Application.OpenURL("https://github.com/Elfinik/burst-trace/blob/main/README-RU.md");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("GitHub", GUILayout.Height(25))) Application.OpenURL("https://github.com/Elfinik/burst-trace");
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            GUILayout.Label("Quick Start", titleStyle);
            GUILayout.Space(10);

            GUILayout.Label("Using (namespace)", textStyle);
            DrawCodeSnippet("using Elfinik.BurstTrace;");

            GUILayout.Label("To capture the call site, use:", textStyle);
            DrawCodeSnippet("var traceHandle = TraceHandle.Capture();");

            GUILayout.Label("You can store the result anywhere:", textStyle);
            DrawCodeSnippet("public TraceHandle traceHandle;");

            GUILayout.Label("To print to the console, call:", textStyle);
            DrawCodeSnippet("Debug.Log(traceHandle.ToProjectLink());");

            GUILayout.Space(15);
            GUILayout.Label("For advanced usage, please refer to the documentation.", textStyle);

            GUI.enabled = previousGuiState;
        }

        private void DrawCodeSnippet(string code)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Box(code, codeStyle, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Copy", GUILayout.Width(70), GUILayout.ExpandHeight(true)))
            {
                EditorGUIUtility.systemCopyBuffer = code;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }
    }
}
#endif