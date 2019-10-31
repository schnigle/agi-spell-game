/*
    Convenience script used to group objects under a new empty game object.
    Source: https://answers.unity.com/questions/118306/grouping-objects-in-the-hierarchy.html
 */

using UnityEditor;
using UnityEngine;

public static class GroupCommand
{
    [MenuItem("GameObject/Group Selected %g")]
    private static void GroupSelected()
    {
        if (!Selection.activeTransform) return;
        var go = new GameObject(Selection.activeTransform.name + " Group");
        Undo.RegisterCreatedObjectUndo(go, "Group Selected");
        go.transform.SetParent(Selection.activeTransform.parent, false);
        foreach (var transform in Selection.transforms) Undo.SetTransformParent(transform, go.transform, "Group Selected");
        Selection.activeGameObject = go;
    }
}