using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Elfinik.BurstTrace.EditorScripts
{
    [CustomPropertyDrawer(typeof(TraceHandle))]
    public class BurstTraceDrawer : PropertyDrawer
    {
        private GUIStyle _messageStyle;

        private GUIStyle MessageStyle
        {
            get
            {
                if (_messageStyle == null)
                {
                    _messageStyle = new GUIStyle(EditorStyles.helpBox);
                    _messageStyle.richText = true;
                    _messageStyle.wordWrap = true;
                    _messageStyle.fontSize = 11;
                    _messageStyle.alignment = TextAnchor.UpperLeft;
                    _messageStyle.padding = new RectOffset(5, 5, 5, 5);
                }
                return _messageStyle;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            TraceHandle stackTrace = GetPropertyValue(property);
            string rawContent = stackTrace.ToProjectLink();

            Rect labelRect = new Rect(position.x + 5, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PrefixLabel(labelRect, label);

            if (!string.IsNullOrEmpty(rawContent))
            {
                float headerHeight = EditorGUIUtility.singleLineHeight + 2;
                Rect contentRect = new Rect(position.x + 5, position.y + headerHeight, position.width, position.height - headerHeight);

                contentRect = EditorGUI.IndentedRect(contentRect);

                EditorGUI.TextArea(contentRect, rawContent, MessageStyle);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TraceHandle stackTrace = GetPropertyValue(property);
            string rawContent = stackTrace.ToProjectLink();

            float headerHeight = EditorGUIUtility.singleLineHeight + 2;

            if (string.IsNullOrEmpty(rawContent))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float indentPixels = EditorGUI.indentLevel * 15f;
            float contentWidth = EditorGUIUtility.currentViewWidth - indentPixels - 30f;

            float contentHeight = MessageStyle.CalcHeight(new GUIContent(rawContent), contentWidth);

            contentHeight = Mathf.Max(contentHeight, EditorGUIUtility.singleLineHeight);

            return headerHeight + contentHeight + 4;
        }

        private TraceHandle GetPropertyValue(SerializedProperty property)
        {
            object target = GetTargetObjectOfProperty(property);
            return (target is TraceHandle handle) ? handle : default;
        }

        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var indexStr = element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", "");
                    var index = Convert.ToInt32(indexStr);

                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;

            if (enumerable is IList list)
            {
                return (index >= 0 && index < list.Count) ? list[index] : null;
            }

            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}