using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LibTessDotNet;
using Unity.VisualScripting.FullSerializer;

public static class UIMeshPainter
{
    //Simple Blue Background
    public static void DrawBackground(VertexHelper vh, Rect rect, Color color)
    {
        int start = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = new Vector2(rect.xMin, rect.yMin);
        vh.AddVert(v);

        v.position = new Vector2(rect.xMin, rect.yMax);
        vh.AddVert(v);

        v.position = new Vector2(rect.xMax, rect.yMax);
        vh.AddVert(v);

        v.position = new Vector2(rect.xMax, rect.yMin);
        vh.AddVert(v);

        vh.AddTriangle(start, start + 1, start + 2);
        vh.AddTriangle(start, start + 2, start + 3);
    }

    //Draw the islands 
    public static void DrawPolygon(VertexHelper vh, List<Vector2> points, Color color)
    {
        if (points == null || points.Count < 3)
            return;

        List<Vector2> cleanPoints = RemoveBadPoints(points, 0.05f);

        if (cleanPoints.Count < 3)
            return;

        Tess tess = new Tess();

        ContourVertex[] contour = new ContourVertex[cleanPoints.Count];

        for (int i = 0; i < cleanPoints.Count; i++)
        {
            contour[i].Position = new Vec3
            {
                X = cleanPoints[i].x,
                Y = cleanPoints[i].y,
                Z = 0f
            };
        }

        tess.AddContour(contour, ContourOrientation.Original);
        tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

        int start = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        for (int i = 0; i < tess.Vertices.Length; i++)
        {
            Vec3 p = tess.Vertices[i].Position;
            v.position = new Vector2(p.X, p.Y);
            vh.AddVert(v);
        }

        for (int i = 0; i < tess.ElementCount; i++)
        {
            int index = i * 3;

            int a = tess.Elements[index];
            int b = tess.Elements[index + 1];
            int c = tess.Elements[index + 2];

            if (a < 0 || b < 0 || c < 0)
                continue;

            vh.AddTriangle(start + a, start + b, start + c);
        }
    }

    static List<Vector2> RemoveBadPoints(List<Vector2> points, float minDistance)
    {
        List<Vector2> result = new List<Vector2>();

        if (points == null || points.Count == 0)
            return result;

        Vector2 last = points[0];
        result.Add(last);

        for (int i = 1; i < points.Count; i++)
        {
            if (Vector2.Distance(last, points[i]) < minDistance)
                continue;

            result.Add(points[i]);
            last = points[i];
        }

        if (result.Count > 2)
        {
            Vector2 first = result[0];
            Vector2 final = result[result.Count - 1];

            if (Vector2.Distance(first, final) < minDistance)
                result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    //Draw the outline of the islands
    public static void DrawSketchStroke(
        VertexHelper vh,
        List<Vector2> points,
        Color color,
        float width,
        float widthNoise,
        float noiseFrequency
    )
    {
        if (points == null || points.Count < 3)
            return;

        int start = vh.currentVertCount;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 prev = points[(i - 1 + points.Count) % points.Count];
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Count];

            Vector2 tangent = (next - prev).normalized;
            Vector2 normal = new Vector2(-tangent.y, tangent.x);

            float noise = Mathf.PerlinNoise(
                current.x * noiseFrequency,
                current.y * noiseFrequency
            );

            float localWidth = width + (noise - 0.5f) * 2f * widthNoise;
            localWidth = Mathf.Max(0.2f, localWidth);

            Color localColor = color;
            localColor.a *= Mathf.Lerp(0.65f, 1f, noise);

            UIVertex v = UIVertex.simpleVert;
            v.color = localColor;

            v.position = current - normal * localWidth * 0.5f;
            vh.AddVert(v);

            v.position = current + normal * localWidth * 0.5f;
            vh.AddVert(v);
        }

        for (int i = 0; i < points.Count; i++)
        {
            int next = (i + 1) % points.Count;

            int a = start + i * 2;
            int b = start + i * 2 + 1;
            int c = start + next * 2;
            int d = start + next * 2 + 1;

            vh.AddTriangle(a, b, d);
            vh.AddTriangle(a, d, c);
        }
    }
    public static void DrawWaterLines(
    VertexHelper vh,
    List<Vector2> islandPoints,
    Color color,
    int lineCount,
    float spacing,
    float width,
    float widthNoise,
    float noiseFrequency
)
    {
        if (islandPoints == null || islandPoints.Count == 0)
            return;

        List<Vector2> cleanPoints = RemoveBadPoints(islandPoints, 0.05f);
        cleanPoints = SmoothClosedLoop(cleanPoints, 3);

        for (int i = 0; i < lineCount; i++)
        {
            float offset = spacing * (i + 1);

            List<Vector2> waterLine = CreateOffsetLoop(
                cleanPoints,
                offset,
                i,
                noiseFrequency
            );

            float fade = 1f - (i / (float)lineCount);

            Color lineColor = color;
            lineColor.a *= fade * 0.65f;

            float localWidth = width * Mathf.Lerp(1f, 0.55f, i / (float)lineCount);

            DrawSketchStroke(
                vh,
                waterLine,
                lineColor,
                localWidth,
                widthNoise,
                noiseFrequency
            );
        }
    }
    static List<Vector2> SmoothClosedLoop(List<Vector2> points, int iterations)
    {
        List<Vector2> result = new List<Vector2>(points);

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            List<Vector2> smoothed = new List<Vector2>();

            for (int i = 0; i < result.Count; i++)
            {
                Vector2 a = result[i];
                Vector2 b = result[(i + 1) % result.Count];

                Vector2 q = Vector2.Lerp(a, b, 0.25f);
                Vector2 r = Vector2.Lerp(a, b, 0.75f);

                smoothed.Add(q);
                smoothed.Add(r);
            }

            result = smoothed;
        }

        return result;
    }
    static List<Vector2> CreateOffsetLoop(
    List<Vector2> points,
    float distance,
    int layerIndex,
    float noiseFrequency
)
    {
        List<Vector2> result = new List<Vector2>();

        int count = points.Count;

        float area = SignedArea(points);

        // If points are clockwise/counter-clockwise, outward normal changes.
        float outwardSign = area > 0f ? -1f : 1f;

        for (int i = 0; i < count; i++)
        {
            Vector2 prev = points[(i - 1 + count) % count];
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % count];

            Vector2 edgeA = (current - prev).normalized;
            Vector2 edgeB = (next - current).normalized;

            Vector2 normalA = new Vector2(-edgeA.y, edgeA.x) * outwardSign;
            Vector2 normalB = new Vector2(-edgeB.y, edgeB.x) * outwardSign;

            Vector2 miter = normalA + normalB;

            if (miter.sqrMagnitude < 0.0001f)
                miter = normalB;
            else
                miter.Normalize();

            float dot = Vector2.Dot(miter, normalB);

            float miterLength = distance;

            if (dot > 0.2f)
                miterLength = distance / dot;

            // Prevent sharp corners from making huge spikes.
            miterLength = Mathf.Clamp(miterLength, 0f, distance * 3f);

            float wobble = Mathf.PerlinNoise(
                current.x * noiseFrequency + layerIndex * 10.13f,
                current.y * noiseFrequency + layerIndex * 7.91f
            );

            float wobbleAmount = (wobble - 0.5f) * distance * 0.35f;

            result.Add(current + miter * (miterLength + wobbleAmount));
        }

        return result;
    }

    static float SignedArea(List<Vector2> points)
    {
        float area = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];

            area += a.x * b.y - b.x * a.y;
        }

        return area * 0.5f;
    }

}