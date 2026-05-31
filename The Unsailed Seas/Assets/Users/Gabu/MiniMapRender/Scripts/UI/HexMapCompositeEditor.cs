using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(HexMapComposite))]
#endif
public class HexMapCompositeEditor
#if UNITY_EDITOR
    : Editor
#endif
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            var composite = (HexMapComposite)target;
            composite.SyncSettings();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Redraw All Chunks"))
        {
            var composite = (HexMapComposite)target;
            composite.SyncSettings();
            composite.RedrawAll();
            EditorUtility.SetDirty(composite);
        }
    }
#endif
}
