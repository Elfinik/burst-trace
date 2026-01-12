using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Elfinik.BurstTrace.Editor
{
    [InitializeOnLoad]
    internal static class BurstTraceHyperlinkHandle
    {
        static BurstTraceHyperlinkHandle()
        {
            EditorGUI.hyperLinkClicked -= OnHyperlinkClicked;
            EditorGUI.hyperLinkClicked += OnHyperlinkClicked;
        }

        private static void OnHyperlinkClicked(object sender, HyperLinkClickedEventArgs e)
        {
            if (e.hyperLinkData.TryGetValue("bstld", out string url))
            {
                if (!uint.TryParse(url, out var value))
                {
                    Debug.LogError($"Link {url} is Invalid!");
                    return;
                }

                var log = new TraceHandle { Value = value };
                Debug.LogError(log.ToProjectLink());
            }
        }
    }
}