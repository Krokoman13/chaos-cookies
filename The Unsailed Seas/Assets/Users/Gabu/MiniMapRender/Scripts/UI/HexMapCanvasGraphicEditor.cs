using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(HexMapCanvasGraphic))]
#endif
public class HexMapCanvasGraphicEditor
#if UNITY_EDITOR
    : Editor
#endif
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        HexMapCanvasGraphic graphic = (HexMapCanvasGraphic)target;

        if (GUILayout.Button("Redraw Same Map"))
        {
            graphic.RedrawSameMap();
            EditorUtility.SetDirty(graphic);
        }
    }
#endif
}