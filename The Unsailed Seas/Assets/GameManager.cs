using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] HexagonTilemap hexagonTilemap;
    [SerializeField] Transform boat;

    [SerializeField] GameObject finalChestPrefab;

    [SerializeField] UnityEvent onLaunch;
    [SerializeField] UnityEvent onRestart;


    public static GameManager instance;

    public void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Duplicate GameManager Found!");
        }

        instance = this;
    }

    public void Restart()
    {
        Setup();
        onRestart?.Invoke();
    }

    public void Setup()
    {
        hexagonTilemap.Generate();
        
        WindManager.instance.RandomizeWind();

        bool[,] hexMap = hexagonTilemap.currentHexMap;
        int maxX = hexMap.GetLength(0);
        int maxY = hexMap.GetLength(1);

        int maxX_fifth = (int)Mathf.Floor((float)maxX / 5.0f);
        int maxY_fifth = (int)Mathf.Floor((float)maxY / 5.0f);

        List<Vector2Int> possibleSpawnLocations = new List<Vector2Int>();
        List<Vector2Int> possibleTreasureLocations = new List<Vector2Int>();

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                if (hexMap[x,y] || HexagonTilemap.ConnectedCount((uint)x, (uint)y, hexMap) > 1) continue;

                Vector2Int point = new Vector2Int(x, y);
                if (x > maxX_fifth && x < maxX_fifth * 4 && y > maxY_fifth && y < maxY_fifth * 4)
                { 
                    possibleSpawnLocations.Add(point);
                }

                possibleTreasureLocations.Add(point);
            }
        }

        {
            Vector2Int randomSpot = possibleSpawnLocations[(int)(possibleSpawnLocations.Count * Random.value)];

            Vector2 randomPosition = HexagonTilemap.CalculateHexPosition((uint)randomSpot.x, (uint)randomSpot.y);

            boat.position = new Vector3(randomPosition.x, boat.position.y, randomPosition.y);
            boat.rotation = Quaternion.Euler(0, WindManager.instance.windAngle_degrees + 90, 0);
        }

        {
            Vector2Int randomSpot = possibleTreasureLocations[(int)(possibleTreasureLocations.Count * Random.value)];

            Vector2 randomPosition = HexagonTilemap.CalculateHexPosition((uint)randomSpot.x, (uint)randomSpot.y);

            GameObject chest = GameObject.Instantiate(finalChestPrefab);
            chest.transform.position = new Vector3(randomPosition.x, boat.position.y, randomPosition.y);
        }

        onLaunch?.Invoke();
    }

    private void Start()
    {
        Setup();
    }
}
