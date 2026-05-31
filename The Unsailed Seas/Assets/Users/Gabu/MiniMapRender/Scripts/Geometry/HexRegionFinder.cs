using UnityEngine;
using System.Collections.Generic;

public static class HexRegionFinder
{
    public static List<List<Vector2Int>> FindConnectedComponents(bool[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> components = new List<List<Vector2Int>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!map[x, y] || visited[x, y])
                    continue;

                components.Add(FloodFill(map, x, y, visited));
            }
        }

        return components;
    }

    static List<Vector2Int> FloodFill(bool[,] map, int startX, int startY, bool[,] visited)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        List<Vector2Int> island = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            island.Add(cell);

            foreach (Vector2Int neighbor in HexGridLayout.GetNeighbors(cell.x, cell.y))
            {
                int nx = neighbor.x;
                int ny = neighbor.y;

                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;

                if (visited[nx, ny] || !map[nx, ny])
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(neighbor);
            }
        }

        return island;
    }
}