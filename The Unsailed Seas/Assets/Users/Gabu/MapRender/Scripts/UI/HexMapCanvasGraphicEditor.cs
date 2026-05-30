using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HexMapCanvasGraphic))]
public class HexMapCanvasGraphicEditor : Editor
{
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
}