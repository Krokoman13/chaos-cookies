using UnityEngine;
using System.Collections.Generic;

public static class HexIslandOutlineBuilder
{
    struct BorderEdge
    {
        public Vector2Int startKey, endKey;
        public Vector2 start, end;
        public bool used;
    }

    struct UndirectedCandidate
    {
        public int edgeIndex;
        public Vector2Int nextKey;
        public Vector2 nextPoint, direction;
        public float turn;
    }

    public static List<Vector2> BuildOutline(
        List<Vector2Int> island, Vector2 offset, float hexSize)
    {
        var edges = CollectBorderEdges(island, offset, hexSize);
        if (edges.Count == 0) return new List<Vector2>();

        var loops = TraceLoops(edges);
        if (loops.Count == 0) loops = TraceLoopsUndirected(edges);

        if (loops.Count == 0)
        {
            Debug.LogWarning($"[Outline] Failed to trace any loop for island with {island.Count} cells, {edges.Count} border edges");
            return new List<Vector2>();
        }

        var biggest = loops[0];
        float biggestArea = Mathf.Abs(SignedArea(biggest));
        for (int i = 1; i < loops.Count; i++)
        {
            float area = Mathf.Abs(SignedArea(loops[i]));
            if (area > biggestArea) { biggest = loops[i]; biggestArea = area; }
        }
        return biggest;
    }

    static List<BorderEdge> CollectBorderEdges(
        List<Vector2Int> island, Vector2 offset, float hexSize)
    {
        var cells = new HashSet<Vector2Int>(island);
        var edges = new List<BorderEdge>();

        foreach (var cell in cells)
        {
            var center = HexGridLayout.HexToCanvasPosition(cell.x, cell.y, hexSize) + offset;
            var corners = HexGridLayout.GetHexCorners(center, hexSize);

            for (int e = 0; e < 6; e++)
            {
                if (cells.Contains(HexGridLayout.GetNeighborForEdge(cell.x, cell.y, e)))
                    continue;

                Vector2 a = corners[e], b = corners[(e + 1) % 6];
                edges.Add(new BorderEdge
                {
                    startKey = PointKey(a), endKey = PointKey(b),
                    start = a, end = b
                });
            }
        }
        DebugBorderGraph(edges);
        return edges;
    }
    static void DebugBorderGraph(List<BorderEdge> edges)
    {
        var degree = new Dictionary<Vector2Int, int>();
        var incoming = new Dictionary<Vector2Int, int>();
        var outgoing = new Dictionary<Vector2Int, int>();

        for (int i = 0; i < edges.Count; i++)
        {
            BorderEdge e = edges[i];

            AddCount(degree, e.startKey);
            AddCount(degree, e.endKey);

            AddCount(outgoing, e.startKey);
            AddCount(incoming, e.endKey);
        }

        int degree1 = 0;
        int degree2 = 0;
        int degreeOther = 0;
        int badDirected = 0;

        foreach (var pair in degree)
        {
            Vector2Int key = pair.Key;
            int d = pair.Value;

            if (d == 1) degree1++;
            else if (d == 2) degree2++;
            else degreeOther++;

            int inCount = incoming.TryGetValue(key, out int inc) ? inc : 0;
            int outCount = outgoing.TryGetValue(key, out int outc) ? outc : 0;

            if (inCount != 1 || outCount != 1)
                badDirected++;
        }

        Debug.Log(
            $"[Outline Graph] Edges={edges.Count} | " +
            $"Vertices={degree.Count} | " +
            $"Degree1={degree1} | " +
            $"Degree2={degree2} | " +
            $"DegreeOther={degreeOther} | " +
            $"BadDirectedVertices={badDirected}"
        );
    }

    static void AddCount(Dictionary<Vector2Int, int> dict, Vector2Int key)
    {
        if (!dict.ContainsKey(key))
            dict[key] = 0;

        dict[key]++;
    }

    // ── Directed loop tracing ──

    static List<List<Vector2>> TraceLoops(List<BorderEdge> edges)
    {
        var byStart = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < edges.Count; i++)
            AddToLookup(byStart, edges[i].startKey, i);

        var loops = new List<List<Vector2>>();
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].used) continue;

            var loop = new List<Vector2>();
            var usedEdges = new List<int>();
            bool closed = TryTraceDirected(
                i, edges[i].startKey, edges, byStart,
                new HashSet<int>(), loop, usedEdges, edges.Count + 10);

            if (!closed || loop.Count < 3) continue;

            foreach (int idx in usedEdges)
            {
                var e = edges[idx]; e.used = true; edges[idx] = e;
            }
            loops.Add(loop);
        }
        return loops;
    }

    static bool TryTraceDirected(
        int edgeIdx, Vector2Int startKey, List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> byStart, HashSet<int> visited,
        List<Vector2> loop, List<int> used, int maxSteps)
    {
        if (visited.Count > maxSteps || visited.Contains(edgeIdx))
            return false;

        var edge = edges[edgeIdx];
        visited.Add(edgeIdx);
        used.Add(edgeIdx);
        loop.Add(edge.start);

        if (edge.endKey == startKey) return true;

        var next = FindNextDirectedEdges(edgeIdx, edge.endKey, edges, byStart, visited);
        for (int i = 0; i < next.Count; i++)
        {
            if (TryTraceDirected(next[i], startKey, edges, byStart, visited, loop, used, maxSteps))
                return true;
        }

        visited.Remove(edgeIdx);
        used.RemoveAt(used.Count - 1);
        loop.RemoveAt(loop.Count - 1);
        return false;
    }

    static List<int> FindNextDirectedEdges(
        int currentIdx, Vector2Int key, List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> byStart, HashSet<int> visited)
    {
        var result = new List<int>();
        if (!byStart.TryGetValue(key, out var candidates)) return result;

        var dir = (edges[currentIdx].end - edges[currentIdx].start).normalized;
        if (dir.sqrMagnitude < 0.0001f) return result;

        for (int i = 0; i < candidates.Count; i++)
        {
            int idx = candidates[i];
            if (edges[idx].used || visited.Contains(idx)) continue;
            if ((edges[idx].end - edges[idx].start).sqrMagnitude < 0.0001f) continue;
            result.Add(idx);
        }

        result.Sort((a, b) => TurnAngle(dir, edges[a]).CompareTo(TurnAngle(dir, edges[b])));
        return result;
    }

    // ── Undirected loop tracing (fallback) ──

    static List<List<Vector2>> TraceLoopsUndirected(List<BorderEdge> edges)
    {
        var byPoint = new Dictionary<Vector2Int, List<int>>();
        for (int i = 0; i < edges.Count; i++)
        {
            AddToLookup(byPoint, edges[i].startKey, i);
            AddToLookup(byPoint, edges[i].endKey, i);
        }

        var globalUsed = new HashSet<int>();
        var loops = new List<List<Vector2>>();

        for (int i = 0; i < edges.Count; i++)
        {
            if (globalUsed.Contains(i)) continue;

            var loop = new List<Vector2> { edges[i].start };
            var usedEdges = new List<int> { i };
            var localUsed = new HashSet<int> { i };

            bool closed = TryTraceUndirected(
                edges[i].startKey, edges[i].endKey, edges[i].end,
                edges[i].end - edges[i].start,
                edges, byPoint, globalUsed, localUsed,
                loop, usedEdges, 0, edges.Count + 10);

            if (!closed || loop.Count < 3) continue;

            foreach (int idx in usedEdges) globalUsed.Add(idx);
            loops.Add(loop);
        }
        return loops;
    }

    static bool TryTraceUndirected(
        Vector2Int startKey, Vector2Int currentKey, Vector2 currentPoint,
        Vector2 currentDir, List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> byPoint,
        HashSet<int> globalUsed, HashSet<int> localUsed,
        List<Vector2> loop, List<int> used, int depth, int maxSteps)
    {
        if (depth > maxSteps) return false;
        if (currentKey == startKey) return true;

        var candidates = FindUndirectedCandidates(
            currentKey, currentDir, edges, byPoint, globalUsed, localUsed);

        for (int i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i];
            localUsed.Add(c.edgeIndex);
            used.Add(c.edgeIndex);
            loop.Add(currentPoint);

            if (TryTraceUndirected(startKey, c.nextKey, c.nextPoint, c.direction,
                    edges, byPoint, globalUsed, localUsed,
                    loop, used, depth + 1, maxSteps))
                return true;

            localUsed.Remove(c.edgeIndex);
            used.RemoveAt(used.Count - 1);
            loop.RemoveAt(loop.Count - 1);
        }
        return false;
    }

    static List<UndirectedCandidate> FindUndirectedCandidates(
        Vector2Int currentKey, Vector2 currentDir, List<BorderEdge> edges,
        Dictionary<Vector2Int, List<int>> byPoint,
        HashSet<int> globalUsed, HashSet<int> localUsed)
    {
        var result = new List<UndirectedCandidate>();
        if (!byPoint.TryGetValue(currentKey, out var candidates)) return result;

        for (int i = 0; i < candidates.Count; i++)
        {
            int idx = candidates[i];
            if (globalUsed.Contains(idx) || localUsed.Contains(idx)) continue;

            var edge = edges[idx];
            Vector2Int nextKey;
            Vector2 nextPoint;

            if (edge.startKey == currentKey)   { nextKey = edge.endKey;   nextPoint = edge.end; }
            else if (edge.endKey == currentKey) { nextKey = edge.startKey; nextPoint = edge.start; }
            else continue;

            Vector2 dir = nextPoint - (edge.startKey == currentKey ? edge.start : edge.end);
            if (dir.sqrMagnitude < 0.0001f) continue;
            dir.Normalize();

            result.Add(new UndirectedCandidate
            {
                edgeIndex = idx, nextKey = nextKey,
                nextPoint = nextPoint, direction = dir,
                turn = TurnAngle(currentDir.normalized, dir)
            });
        }

        result.Sort((a, b) => a.turn.CompareTo(b.turn));
        return result;
    }

    // ── Utilities ──

    static float TurnAngle(Vector2 from, BorderEdge edge)
    {
        return TurnAngle(from, (edge.end - edge.start).normalized);
    }

    static float TurnAngle(Vector2 from, Vector2 to)
    {
        float angle = Vector2.SignedAngle(from, to);
        return angle < 0f ? angle + 360f : angle;
    }

    static float SignedArea(List<Vector2> pts)
    {
        float area = 0f;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 a = pts[i], b = pts[(i + 1) % pts.Count];
            area += a.x * b.y - b.x * a.y;
        }
        return area * 0.5f;
    }

    static Vector2Int PointKey(Vector2 p)
    {
        const float scale = 100f;
        return new Vector2Int(Mathf.RoundToInt(p.x * scale), Mathf.RoundToInt(p.y * scale));
    }

    static void AddToLookup(Dictionary<Vector2Int, List<int>> dict, Vector2Int key, int value)
    {
        if (!dict.ContainsKey(key)) dict[key] = new List<int>();
        dict[key].Add(value);
    }
}
