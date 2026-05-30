using UnityEngine;
using System.Collections.Generic;

public static class HexGridLayout
{
    const float RowHeight = 0.8660254f;

    public static Vector2 HexToCanvasPosition(int x, int y, float hexSize)
    {
        float px = x * hexSize;
        float py = y * RowHeight * hexSize;

        if (y % 2 == 1)
            px += 0.5f * hexSize;

        return new Vector2(px, py);
    }

    public static Vector2 GetMapPixelSize(int width, int height, float hexSize)
    {
        return new Vector2(
            (width + 0.5f) * hexSize,
            height * RowHeight * hexSize
        );
    }

    public static Vector2[] GetHexCorners(Vector2 center, float hexSize)
    {
        Vector2[] corners = new Vector2[6];

        float radius = hexSize / Mathf.Sqrt(3f);

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (90f + 60f * i);

            corners[i] = center + new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;
        }

        return corners;
    }

    public static IEnumerable<Vector2Int> GetNeighbors(int x, int y)
    {
        if (y % 2 == 0)
        {
            yield return new Vector2Int(x - 1, y);
            yield return new Vector2Int(x + 1, y);
            yield return new Vector2Int(x, y - 1);
            yield return new Vector2Int(x - 1, y - 1);
            yield return new Vector2Int(x, y + 1);
            yield return new Vector2Int(x - 1, y + 1);
        }
        else
        {
            yield return new Vector2Int(x - 1, y);
            yield return new Vector2Int(x + 1, y);
            yield return new Vector2Int(x + 1, y - 1);
            yield return new Vector2Int(x, y - 1);
            yield return new Vector2Int(x + 1, y + 1);
            yield return new Vector2Int(x, y + 1);
        }
    }

    public static Vector2Int GetNeighborForEdge(int x, int y, int edge)
    {
        bool odd = y % 2 == 1;

        if (!odd)
        {
            switch (edge)
            {
                case 0: return new Vector2Int(x - 1, y + 1);
                case 1: return new Vector2Int(x - 1, y);
                case 2: return new Vector2Int(x - 1, y - 1);
                case 3: return new Vector2Int(x, y - 1);
                case 4: return new Vector2Int(x + 1, y);
                case 5: return new Vector2Int(x, y + 1);
            }
        }
        else
        {
            switch (edge)
            {
                case 0: return new Vector2Int(x, y + 1);
                case 1: return new Vector2Int(x - 1, y);
                case 2: return new Vector2Int(x, y - 1);
                case 3: return new Vector2Int(x + 1, y - 1);
                case 4: return new Vector2Int(x + 1, y);
                case 5: return new Vector2Int(x + 1, y + 1);
            }
        }

        return new Vector2Int(x, y);
    }
}