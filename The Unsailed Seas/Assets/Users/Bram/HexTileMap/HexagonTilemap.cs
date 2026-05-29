using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HexagonTilemap : MonoBehaviour
{
    [SerializeField] GameObject beachHexagon;
    [SerializeField] GameObject seaHexagon;

    [SerializeField] int maxX = 192;
    [SerializeField] int maxY = 108;

    public bool[,] currentHexMap = null;

    [Serializable] enum Mode { SpawnBeach = 0, RemoveBeach = 1}
    [Serializable] struct TerraformRule
    {
        public Mode mode;
        public float chance;
        public uint minConnected;
    }

    [SerializeField] List<TerraformRule> terraformRules = new List<TerraformRule>();

    public void Generate()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

        currentHexMap = new bool[maxX, maxY];

        foreach (TerraformRule rule in terraformRules)
        {
            List<Vector2Int> toChange = new List<Vector2Int>();
            for (uint i = 0; i < maxX; i++)
            {
                for (uint j = 0; j < maxY; j++)
                {
                    if (rule.mode == Mode.SpawnBeach && currentHexMap[i, j]) continue;
                    if (rule.mode == Mode.RemoveBeach && !currentHexMap[i, j]) continue;
                    if (rule.minConnected > ConnectedCount(i, j, currentHexMap)) continue;
                    if (UnityEngine.Random.value > rule.chance) continue;

                    toChange.Add(new Vector2Int((int)i, (int)j));
                }
            }

            foreach (Vector2Int position in toChange)
            {
                if (rule.mode == Mode.SpawnBeach)
                {
                    currentHexMap[position.x, position.y] = true;
                }
                else if (rule.mode == Mode.RemoveBeach)
                {
                    currentHexMap[position.x, position.y] = false;
                }
            }
        }

        for (uint i = 0; i < maxX; i++)
        {
            for (uint j = 0; j < maxY; j++)
            {
                GameObject prefab = currentHexMap[i, j] ? beachHexagon : seaHexagon;

                if (prefab == null) continue;
                GameObject hexagon = Instantiate(prefab, this.transform);
                Vector2 horizontalPosition = CalculateHexPosition(i, j);
                hexagon.transform.position = new Vector3(horizontalPosition.x, hexagon.transform.position.y, horizontalPosition.y);

            }
        }
    }

    List<bool> ConnectedHexTiles(uint x, uint y, bool[,] hexMap)
    {
        int maxX = hexMap.GetLength(0);
        int maxY = hexMap.GetLength(1);

        List<bool> connected = new();

        void AddIfValid(uint nx, uint ny)
        {
            if (nx >= 0 && nx < maxX &&
                ny >= 0 && ny < maxY)
            {
                connected.Add(hexMap[nx, ny]);
            }
            else
            {
                connected.Add(false);
            }
        }

        if (y % 2 == 0)
        {
            AddIfValid(x - 1, y + 1);
            AddIfValid(x, y + 1);
            AddIfValid(x - 1, y);
            AddIfValid(x + 1, y);
            AddIfValid(x - 1, y - 1);
            AddIfValid(x, y - 1);
        }
        else
        {
            AddIfValid(x, y + 1);
            AddIfValid(x + 1, y + 1);
            AddIfValid(x - 1, y);
            AddIfValid(x + 1, y);
            AddIfValid(x, y - 1);
            AddIfValid(x + 1, y - 1);
        }

        return connected;
    }

    uint ConnectedCount(uint x, uint y, bool[,] hexMap)
    {
        List<bool> connectedHexTiles = ConnectedHexTiles(x, y, hexMap);

        uint connectedCount = 0;
        foreach (bool b in connectedHexTiles)
        {
            if (!b) continue;
            connectedCount++;
        }

        return connectedCount;
    }

    Vector2 CalculateHexPosition(uint x, uint y)
    {
        const float sqrtThree = 0.8660254f;
        Vector2 outPosition = new Vector2(x, y);

        outPosition.y *= sqrtThree;

        if (y % 2 == 1)
        {
            outPosition.x += 0.5f;
        }

        return outPosition;
    }
}
