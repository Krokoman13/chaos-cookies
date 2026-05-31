using UnityEngine;
using System.Collections.Generic;

public static class HexIslandOutlineBuilder
{
    struct BorderEdge
    {
        public Vector2Int startKey;
        public Vector2Int endKey;
        public Vector2 start;
        public Vector2 end;
        public bool used;
    }

    public static List<Vector2> BuildOutline(
        List<Vector2Int> island,
        Vector2 offset,
        float hexSize
    )
    {
        List<BorderEdge> edges = CollectBorderEdges(island, offset, hexSize);

        if (edges.Count == 0)
            return new List<Vector2>();

        List<List<Vector2>> loops = TraceLoops(edges);

        if (loops.Count == 0)
            return new List<Vector2>();

        // If there are multiple loops, keep the biggest one.
        // Later, holes / lakes can be handled separately.
        List<Vector2> biggest = loops[0];
        float biggestArea = Mathf.Abs(SignedArea(biggest));

        for (int i = 1; i < loops.Count; i++)
        {
            float area = Mathf.Abs(SignedArea(loops[i]));

            if (area > biggestArea)
            {
                biggest = loops[i];
                biggestArea = area;
            }
        }

        return biggest;
    }

    static List<BorderEdge> CollectBorderEdges(
        List<Vector2Int> island,
        Vector2 offset,
        float hexSize
    )
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>(island);
        List<BorderEdge> edges = new List<BorderEdge>();

        foreach (Vector2Int cell in island)
        {
            Vector2 center = HexGridLayout.HexToCanvasPosition(cell.x, cell.y, hexSize) + offset;
            Vector2[] corners = HexGridLayout.GetHexCorners(center, hexSize);

            for (int edge = 0; edge < 6; edge++)
            {
                Vector2Int neighbor = HexGridLayout.GetNeighborForEdge(cell.x, cell.y, edge);

                // If another island cell is on this side, this edge is internal.
                if (cells.Contains(neighbor))
                    continue;

                Vector2 a = corners[edge];
                Vector2 b = corners[(edge + 1) % 6];

                edges.Add(new BorderEdge
                {
                    startKey = PointKey(a),
                    endKey = PointKey(b),
                    start = a,
                    end = b,
                    used = false
                });
            }
        }

        return edges;
    }

    static List<List<Vector2>> TraceLoops(List<BorderEdge> edges)
    {
        Dictionary<Vector2Int, List<int>> edgesByStart = new Dictionary<Vector2Int, List<int>>();

        for (int i = 0; i < edges.Count; i++)
        {
            Vector2Int key = edges[i].startKey;

            if (!edgesByStart.ContainsKey(key))
                edgesByStart[key] = new List<int>();

            edgesByStart[key].Add(i);
        }

        List<List<Vector2>> loops = new List<List<Vector2>>();

        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].used)
                continue;

            List<Vector2> loop = TraceSingleLoop(i, edges, edgesByStart);

            if (loop.Count >= 3)
                loops.Add(loop);
        }

        return loops;
    }

    static List<Vector2> TraceSingleLoop(
        int startEdgeIndex,
        List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> edgesByStart
    )
    {
        List<Vector2> loop = new List<Vector2>();

        int currentEdgeIndex = startEdgeIndex;
        Vector2Int startKey = edges[startEdgeIndex].startKey;

        int safety = 0;
        int maxSteps = edges.Count + 10;

        while (safety++ < maxSteps)
        {
            BorderEdge edge = edges[currentEdgeIndex];

            edge.used = true;
            edges[currentEdgeIndex] = edge;

            loop.Add(edge.start);

            Vector2Int nextKey = edge.endKey;

            if (nextKey == startKey)
                break;

            int nextEdgeIndex = FindUnusedEdgeStartingAt(nextKey, edges, edgesByStart);

            if (nextEdgeIndex == -1)
                break;

            currentEdgeIndex = nextEdgeIndex;
        }

        return loop;
    }

    static int FindUnusedEdgeStartingAt(
        Vector2Int key,
        List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> edgesByStart
    )
    {
        if (!edgesByStart.TryGetValue(key, out List<int> candidates))
            return -1;

        for (int i = 0; i < candidates.Count; i++)
        {
            int edgeIndex = candidates[i];

            if (!edges[edgeIndex].used)
                return edgeIndex;
        }

        return -1;
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

    static Vector2Int PointKey(Vector2 point)
    {
        const float scale = 1000f;

        return new Vector2Int(
            Mathf.RoundToInt(point.x * scale),
            Mathf.RoundToInt(point.y * scale)
        );
    }
}