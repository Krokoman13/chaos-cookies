using UnityEngine;
using UnityEngine.UI;
using System;

public class HexMapCanvasGraphic : MaskableGraphic
{
    [Header("Map Data")]
    [SerializeField] HexagonTilemap source;

    [Header("Hex Style")]
    [SerializeField] float hexSize = 10f;
    [SerializeField] Material seaMaterial;
    [SerializeField] Material beachMaterial;

    bool[,] map;

    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();

        if (source)
        {
            source.OnMapChanged += SetMap;

            if (source.currentHexMap != null)
                SetMap(source.currentHexMap);
        }
    }

    protected override void OnDisable()
    {
        if (source)
            source.OnMapChanged -= SetMap;

        base.OnDisable();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (map == null)
            return;

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Vector2 mapSize = GetMapPixelSize(width, height);
        Vector2 offset = -mapSize * 0.5f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 center = HexToCanvasPosition(x, y) + offset;
                DrawHex(vh, center, hexSize, map[x, y] ? beachMaterial : seaMaterial);
            }
        }
    }

    Vector2 HexToCanvasPosition(int x, int y)
    {
        const float rowHeight = 0.8660254f;

        float px = x * hexSize;
        float py = y * rowHeight * hexSize;

        if (y % 2 == 1)
            px += 0.5f * hexSize;

        return new Vector2(px, py);
    }

    Vector2 GetMapPixelSize(int width, int height)
    {
        const float rowHeight = 0.8660254f;
        return new Vector2((width + 0.5f) * hexSize, height * rowHeight * hexSize);
    }   

    void DrawHex(VertexHelper vh, Vector2 center, float size, Material material)
    {
        int start = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;

        v.color = material.color;
        v.position = center;
        vh.AddVert(v);

        float radius = size / Mathf.Sqrt(3f);
        radius *= 1.02f;
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + 60f * i);
            v.position = center + new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;
            vh.AddVert(v);
        }
        for (int i = 0; i < 6; i++)
        {
            vh.AddTriangle(
                start,
                start + i + 1,
                start + (i == 5 ? 1 : i + 2)
            );
        }
    }
    public void SetMap(bool[,] newMap)
    {
        map = newMap;
        SetVerticesDirty();
    }

}