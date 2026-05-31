using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class HexMapCanvasGraphic : MaskableGraphic
{
    [Header("Layout")]
    public float HexSize = 10f;

    [Header("Colors")]
    public Color SeaColor = new Color(0.1f, 0.35f, 0.75f, 1f);
    public Color LandColor = new Color(0.85f, 0.72f, 0.45f, 1f);
 
    [Header("Coastline")]
    public Color CoastColor = new Color(0.12f, 0.08f, 0.04f, 1f);
    public float MainWidth = 1.4f;
    public float MainWidthNoise = 0.35f;
    public float MainNoiseFrequency = 0.08f;
    public bool DrawSecondaryStroke = true;
    public float SecondaryAlpha = 0.45f;
    public float SecondaryWidth = 0.75f;
    public float SecondaryWidthNoise = 0.25f;
    public float SecondaryNoiseFrequency = 0.12f;
    public float SecondaryWobbleAmount = 1.2f;
    public float SecondaryWobbleFrequency = 0.05f;

    [Header("Waves")]
    public Color WavesColor = new Color(1f, 1f, 1f, 0.35f);
    public int WavesCount = 3;
    public float WavesSpacing = 7f;
    public float WavesWidth = 1.4f;
    public float WavesWidthNoise = 0.6f;
    public float WavesNoiseFrequency = 0.025f;

    [Header("Simplification")]
    public int MinIslandSize = 4;
    public int SmoothingIterations = 1;

    [Header("Chunking")]
    public int ChunkIndex = 0;
    public int ChunkCount = 1;
    public bool DrawBackgroundInThisChunk = true;

    [SerializeField] HexagonTilemap source;

    bool[,] map;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Debug.Log($"{name}: HexMapCanvasGraphic OnEnable");

        if (!source)
        {
            Debug.LogWarning($"{name}: source is NULL");
            return;
        }

        Debug.Log($"{name}: source assigned = {source.name}");
        Debug.Log($"{name}: source.currentHexMap is {(source.currentHexMap == null ? "NULL" : "NOT NULL")}");

        source.OnMapChanged += SetMap;

        if (source.currentHexMap != null)
            SetMap(source.currentHexMap);
    }

    protected override void OnDisable()
    {
        if (source)
            source.OnMapChanged -= SetMap;

        base.OnDisable();
    }

    public void SetMap(bool[,] newMap)
    {
        Debug.Log($"{name}: SetMap called. newMap is {(newMap == null ? "NULL" : "NOT NULL")}");

        map = newMap;
        SetVerticesDirty();
    }
    public void RedrawSameMap()
    {
        if (source != null && source.currentHexMap != null)
            map = source.currentHexMap;

        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (DrawBackgroundInThisChunk)
            UIMeshPainter.DrawBackground(vh, rectTransform.rect, SeaColor);

        if (map == null)
            return;

        Vector2 mapSize = HexGridLayout.GetMapPixelSize(
            map.GetLength(0),
            map.GetLength(1),
            HexSize
        );

        Vector2 offset = -mapSize * 0.5f;

        List<List<Vector2Int>> islands = HexRegionFinder.FindConnectedComponents(map);
        for (int islandIndex = 0; islandIndex < islands.Count; islandIndex++)
        {
            if (ChunkCount > 1 && islandIndex % ChunkCount != ChunkIndex)
                continue;

            List<Vector2Int> island = islands[islandIndex];

            if (island.Count < MinIslandSize)
                continue;

            List<Vector2> outline = HexIslandOutlineBuilder.BuildOutline(
                island,
                offset,
                HexSize
            );
            // We draw water lines even if islands are too small so we can still see a glimpse of those 
            UIMeshPainter.DrawWaterLines(vh, outline, WavesColor, WavesCount, WavesSpacing, WavesWidth, WavesWidthNoise, WavesNoiseFrequency);


            if (outline.Count < 3)
                continue;

            outline = PolylineSmoother.SmoothClosedLoop(
                outline,
                SmoothingIterations
            );

            UIMeshPainter.DrawPolygon(vh, outline, LandColor);
            UIMeshPainter.DrawSketchStroke(
                vh,
                outline,
                CoastColor,
                MainWidth,
                MainWidthNoise,
                MainNoiseFrequency
            );

            List<Vector2> wobbleA = PolylineSmoother.WobbleClosedLoop(outline, SecondaryWobbleAmount, SecondaryWobbleFrequency);

            Color faded = CoastColor;
            faded.a = SecondaryAlpha;
            if (DrawSecondaryStroke)
            {
                UIMeshPainter.DrawSketchStroke(
                    vh,
                    wobbleA,
                    faded,
                    SecondaryWidth,
                    SecondaryWidthNoise,
                    SecondaryNoiseFrequency
                );
            }
        }

#if UNITY_EDITOR
        if (vh.currentVertCount > 60000)
            Debug.LogWarning($"{name} generated {vh.currentVertCount} UI vertices. Split into more chunks.");
#endif
    }
}