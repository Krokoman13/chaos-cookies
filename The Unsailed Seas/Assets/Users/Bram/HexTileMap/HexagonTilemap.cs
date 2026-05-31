using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HexagonTilemap : MonoBehaviour
{
    [SerializeField] int maxX = 192;
    [SerializeField] int maxY = 108;

    public bool[,] currentHexMap = null;
    public event Action<bool[,]> MapChanged;

    [Serializable] enum Mode { SpawnBeach = 0, RemoveBeach = 1}
    [Serializable] struct TerraformRule
    {
        public Mode mode;
        public float chance;
        public uint minConnected;
    }

    [SerializeField] List<TerraformRule> terraformRules = new List<TerraformRule>();

    [Serializable]
    class BeachTile
    {
        public GameObject prefab;
        public List<bool> connections;
        public int yRotation;  //0-5 multiplied by 30
    }

    [SerializeField] List<BeachTile> beachTiles = new List<BeachTile>();

    private void RotateBeachTiles()
    {
        if (beachTiles.Count < 64) return;

        // Only iterate over the original tiles
        int originalCount = beachTiles.Count;

        for (int x = 0; x < originalCount; x++)
        {
            BeachTile sourceTile = beachTiles[x];

            // Generate the other 5 rotations
            for (int rotationOffset = 1; rotationOffset < 6; rotationOffset++)
            {
                List<bool> newConnections = RotateConnections(
                    sourceTile.connections,
                    rotationOffset
                );

                int newYRotation = (sourceTile.yRotation + rotationOffset) % 6;

                bool found = false;

                foreach (BeachTile existingTile in beachTiles)
                {
                    if (ConnectionsEqual(existingTile.connections, newConnections))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                    continue;

                BeachTile tile = new BeachTile();
                tile.connections = newConnections;
                tile.yRotation = newYRotation;
                tile.prefab = sourceTile.prefab;

                beachTiles.Add(tile);
            }
        }
    }

    private List<bool> RotateConnections(List<bool> connections, int steps)
    {
        List<bool> result = new List<bool>(new bool[6]);

        for (int i = 0; i < 6; i++)
        {
            result[(i + steps) % 6] = connections[i];
        }

        return result;
    }

    private bool ConnectionsEqual(List<bool> a, List<bool> b)
    {
        if (a.Count != b.Count)
            return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    public void Generate()
    {
        RotateBeachTiles();

        List<Transform> toDestroy = new List<Transform>();

        foreach (Transform child in transform)
        { 
            toDestroy.Add(child);    
        }

        foreach (Transform child in toDestroy)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
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
                if (!currentHexMap[i, j]) continue;

                BeachTile tile = null;
                List<bool> connections = ConnectedHexTiles(i, j, currentHexMap);

                foreach (BeachTile beachTile in beachTiles)
                {
                    if (!ConnectionsEqual(connections, beachTile.connections)) continue;

                    tile = beachTile;
                    break;
                }

                string newName = "(";

                foreach (bool boolean in connections)
                {
                    newName += boolean ? "1," : "0,";
                }

                newName += ")";

                if (tile == null)
                {
                    string debugMessage = "ERRROR: Connections: " + newName + " not found";
                    Debug.LogWarning(debugMessage);
                    continue;
                }

                GameObject hexagon = Instantiate(tile.prefab, this.transform);
                hexagon.name = newName;
                hexagon.transform.rotation = Quaternion.Euler(0,(tile.yRotation + 2) * 60.0f,0);
                Vector2 horizontalPosition = CalculateHexPosition(i, j);
                hexagon.transform.position = new Vector3(horizontalPosition.x, hexagon.transform.position.y, horizontalPosition.y);

            }
        }

        MapChanged?.Invoke(currentHexMap);
    }

    private void OnValidate()
    {
        foreach (BeachTile beach in beachTiles)
        {
            if (beach.connections.Count != 6)
            { 
                beach.connections = new List<bool>(6) {
                    false, false, false,
                    false, false, false
                };
            }
        }
    }

    static List<bool> ConnectedHexTiles(uint x, uint y, bool[,] hexMap)
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
            AddIfValid(x + 1, y);

            AddIfValid(x, y - 1);
            AddIfValid(x -1, y - 1);
            AddIfValid(x - 1, y);
        }
        else
        {
            AddIfValid(x, y + 1);
            AddIfValid(x + 1, y + 1);
            AddIfValid(x + 1, y);

            AddIfValid(x + 1, y - 1);
            AddIfValid(x, y -1);
            AddIfValid(x - 1, y);
        }

        return connected;
    }

    public static uint ConnectedCount(uint x, uint y, bool[,] hexMap)
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

    public static Vector2 CalculateHexPosition(uint x, uint y)
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

    private void OnDrawGizmosSelected()
    {
        return;

#if UNITY_EDITOR
        for (uint i = 0; i < maxX; i++)
        {
            for (uint j = 0; j < maxY; j++)
            {
                Vector2 horizontalPosition = CalculateHexPosition(i, j);
                Vector3 position = new Vector3(horizontalPosition.x, 0, horizontalPosition.y);
                Handles.Label(position, i + "," + j);
            }
        }
#endif
    }
}
