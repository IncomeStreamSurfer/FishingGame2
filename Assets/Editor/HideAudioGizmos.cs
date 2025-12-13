using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

/// <summary>
/// Hides AudioSource and Light icons in the Scene view
/// </summary>
[InitializeOnLoad]
public static class HideAudioGizmos
{
    static HideAudioGizmos()
    {
        // Run on editor load
        EditorApplication.delayCall += HideAllIcons;
    }

    [MenuItem("Tools/Hide Audio & Light Gizmos")]
    static void HideAllIcons()
    {
        // AudioSource ClassID = 82
        SetGizmoIconEnabled(82, false);
        // Light ClassID = 108
        SetGizmoIconEnabled(108, false);
        Debug.Log("Audio and Light gizmos hidden");
    }

    [MenuItem("Tools/Show Audio & Light Gizmos")]
    static void ShowAllIcons()
    {
        SetGizmoIconEnabled(82, true);
        SetGizmoIconEnabled(108, true);
        Debug.Log("Audio and Light gizmos shown");
    }

    static void SetGizmoIconEnabled(int classId, bool enabled)
    {
        // Access Unity's internal AnnotationUtility to toggle gizmo icons
        var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
        if (annotationUtility == null) return;

        var setIconEnabled = annotationUtility.GetMethod("SetIconEnabled",
            BindingFlags.Static | BindingFlags.NonPublic);

        if (setIconEnabled == null) return;

        setIconEnabled.Invoke(null, new object[] { classId, "", enabled ? 1 : 0 });
    }
}
