#if BURST_TRACE_ENTITIES_SUPPORT
using Unity.Entities.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Elfinik.BurstTrace.Entities.Editor
{
    class BurstTraceECSInspectorDrawer : PropertyInspector<TraceHandle>
    {
        public IMGUIContainer container;
        public override VisualElement Build()
        {
            var res = new IMGUIContainer(OnGUI);
            container = res;
            return res;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(Name), EditorStyles.boldLabel);
            string rawContent = Target.ToProjectLink();
            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            float height = myStyle.CalcHeight(new GUIContent(rawContent), EditorGUIUtility.currentViewWidth);
            EditorGUILayout.TextArea(rawContent, myStyle, GUILayout.Height(height + 10));
        }
    }
}
#endif