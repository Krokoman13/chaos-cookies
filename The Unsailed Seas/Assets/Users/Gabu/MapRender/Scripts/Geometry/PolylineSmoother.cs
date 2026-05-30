using UnityEngine;
using System.Collections.Generic;

public static class PolylineSmoother
{
    public static List<Vector2> SmoothClosedLoop(List<Vector2> points, int iterations)
    {
        if (points == null || points.Count < 3)
            return points;

        List<Vector2> result = new List<Vector2>(points);

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            List<Vector2> smoothed = new List<Vector2>(result.Count * 2);

            for (int i = 0; i < result.Count; i++)
            {
                Vector2 a = result[i];
                Vector2 b = result[(i + 1) % result.Count];

                Vector2 q = Vector2.Lerp(a, b, 0.33f);
                Vector2 r = Vector2.Lerp(a, b, 0.66f);

                smoothed.Add(q);
                smoothed.Add(r);
            }

            result = smoothed;
        }

        return result;
    }
    public static List<Vector2> WobbleClosedLoop(
        List<Vector2> points,
        float amount,
        float frequency
    )
    {
        if (points == null || points.Count < 3)
            return points;

        List<Vector2> result = new List<Vector2>(points.Count);

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 previous = points[(i - 1 + points.Count) % points.Count];
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Count];

            Vector2 tangent = (next - previous).normalized;
            Vector2 normal = new Vector2(-tangent.y, tangent.x);

            float noise = Mathf.PerlinNoise(
                current.x * frequency,
                current.y * frequency
            );

            float offset = (noise - 0.5f) * 2f * amount;

            result.Add(current + normal * offset);
        }

        return result;
    }
}