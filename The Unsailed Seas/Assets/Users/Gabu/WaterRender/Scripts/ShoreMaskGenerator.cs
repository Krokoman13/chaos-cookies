using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;
using System.IO;

[ExecuteAlways]
public class ShoreMaskGenerator : MonoBehaviour
{

    [Header("Debug")]
    [SerializeField] RawImage debugPreview;
    [SerializeField] bool saveDebugPng = true;

    public HexagonTilemap Source;
    public MeshRenderer WaterRenderer;

    public Material WaterMaterial;

    public float HexSize = 10f;
    public int TextureSize = 1024;
    public float ShoreDistance = 24f;
    public int MinIslandSize = 4;
    bool subscribed;

    Texture2D shoreTexture;
    void Awake()
    {
        if (WaterRenderer != null)
            WaterMaterial = WaterRenderer.sharedMaterial;
    }
    void Start()
    {
        Subscribe();
        Rebuild();
    }

    void OnEnable()
    {
        if (Source != null)
            Source.OnMapChanged += HandleMapChanged;
        Subscribe();
        Rebuild();
    }

    void OnDisable()
    {
        if (Source != null)
            Source.OnMapChanged -= HandleMapChanged;
    }

    void HandleMapChanged(bool[,] newMap)
    {
        Debug.Log($"{name}: OnMapChanged received. Rebuilding shore mask.");
        Rebuild();
    }

    void Subscribe()        
    {
        if (subscribed)
            return;

        if (Source == null)
        {
            Debug.LogWarning($"{name}: ShoreMaskGenerator has no Source assigned.");
            return;
        }

        Source.OnMapChanged += HandleMapChanged;
        subscribed = true;

        Debug.Log($"{name}: Subscribed to {Source.name}.OnMapChanged.");
    }

    [ContextMenu("Force Rebuild Shore Mask")]
    public void Rebuild()
    {
        Debug.Log($"{name}: Rebuild called.");
        if (Source == null || Source.currentHexMap == null || WaterMaterial == null)
        {
            if (Source == null)
                Debug.Log("Source null");
            if (Source.currentHexMap == null)
                Debug.Log("HexMap null");

            return; 
        }


        bool[,] map = Source.currentHexMap;

        Vector2 mapSize = HexGridLayout.GetMapPixelSize(
            map.GetLength(0),
            map.GetLength(1),
            HexSize
        );

        Vector2 offset = -mapSize * 0.5f;

        List<List<Vector2>> outlines = new List<List<Vector2>>();

        List<List<Vector2Int>> islands = HexRegionFinder.FindConnectedComponents(map);

        foreach (List<Vector2Int> island in islands)
        {
            if (island.Count < MinIslandSize)
                continue;

            List<Vector2> outline = HexIslandOutlineBuilder.BuildOutline(
                island,
                offset,
                HexSize
            );

            if (outline.Count >= 3)
                outlines.Add(outline);
        }

        if (shoreTexture == null || shoreTexture.width != TextureSize)
        {
            shoreTexture = new Texture2D(
                TextureSize,
                TextureSize,
                TextureFormat.R8,
                false,
                true
            );

            shoreTexture.wrapMode = TextureWrapMode.Clamp;
            shoreTexture.filterMode = FilterMode.Bilinear;
        }

        Color[] pixels = new Color[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                Vector2 uv = new Vector2(
                    (x + 0.5f) / TextureSize,
                    (y + 0.5f) / TextureSize
                );

                Vector2 p = new Vector2(
                    Mathf.Lerp(offset.x, offset.x + mapSize.x, uv.x),
                    Mathf.Lerp(offset.y, offset.y + mapSize.y, uv.y)
                );

                float nearest = float.MaxValue;

                foreach (List<Vector2> outline in outlines)
                {
                    for (int i = 0; i < outline.Count; i++)
                    {
                        Vector2 a = outline[i];
                        Vector2 b = outline[(i + 1) % outline.Count];

                        float d = DistancePointSegment(p, a, b);
                        nearest = Mathf.Min(nearest, d);
                    }
                }

                float shore = Mathf.Clamp01(1f - nearest / ShoreDistance);

                pixels[y * TextureSize + x] = new Color(shore, shore, shore, 1f);
            }
        }

        shoreTexture.SetPixels(pixels);
        shoreTexture.Apply(false, false);
        if (debugPreview != null)
        {
            debugPreview.texture = shoreTexture;
            debugPreview.color = Color.white;
        }

        DebugTextureStats(pixels);

        #if UNITY_EDITOR
                if (saveDebugPng)
                {
                    byte[] png = shoreTexture.EncodeToPNG();
                    File.WriteAllBytes("Assets/ShoreMaskDebug.png", png);
                    AssetDatabase.Refresh();
                    Debug.Log("Saved shore mask debug texture to Assets/ShoreMaskDebug.png");
                }
        #endif
        WaterMaterial.SetTexture("_ShoreMaskTex", shoreTexture);
        WaterMaterial.SetVector("_ShoreMapOrigin", new Vector4(offset.x, offset.y, 0, 0));
        WaterMaterial.SetVector("_ShoreMapSize", new Vector4(mapSize.x, mapSize.y, 0, 0));
        Debug.Log($"{name}: Rebuild finished.");
    }
    void DebugTextureStats(Color[] pixels)
    {
        float min = 999f;
        float max = -999f;
        float sum = 0f;

        for (int i = 0; i < pixels.Length; i++)
        {
            float v = pixels[i].r;
            min = Mathf.Min(min, v);
            max = Mathf.Max(max, v);
            sum += v;
        }

        float avg = sum / pixels.Length;

        Debug.Log($"Shore mask stats: min={min:F3}, max={max:F3}, avg={avg:F3}");
    }
    static float DistancePointSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 0.0001f);
        t = Mathf.Clamp01(t);

        return Vector2.Distance(p, a + ab * t);
    }
}