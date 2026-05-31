using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexMapComposite))]
public class HexMapCompositeEditor : Editor
{
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
}
