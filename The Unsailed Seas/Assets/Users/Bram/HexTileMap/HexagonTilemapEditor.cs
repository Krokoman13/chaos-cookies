#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(HexagonTilemap))]
#endif
public class HexagonTilemapEditor
#if UNITY_EDITOR
    : Editor
#endif
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            ((HexagonTilemap)target).Generate();
        }
    }
#endif
}
