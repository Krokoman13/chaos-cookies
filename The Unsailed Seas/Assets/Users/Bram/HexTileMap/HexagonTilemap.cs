using NUnit.Framework;
using UnityEngine;

public class HexagonTilemap : MonoBehaviour
{
    [SerializeField] GameObject beachHexagon;
    [SerializeField] GameObject seaHexagon;

    [SerializeField] int maxX = 192;
    [SerializeField] int maxY = 108;

    public bool[,] currentHexMap = null;

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

        for (uint i = 0; i < maxX; i++)
        {
            for (uint j = 0; j < maxY; j++)
            {
                currentHexMap[i, j] = Random.value > 0.90f;
            }
        }

        for (uint i = 0; i < maxX; i++)
        {
            for (uint j = 0; j < maxY; j++)
            {
                GameObject hexagon = Instantiate(currentHexMap[i, j] ? beachHexagon : seaHexagon, this.transform);
                Vector2 horizontalPosition = calculateHexPosition(i, j);
                hexagon.transform.position = new Vector3(horizontalPosition.x, hexagon.transform.position.y, horizontalPosition.y);

            }
        }
    }

    Vector2 calculateHexPosition(uint x, uint y)
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
