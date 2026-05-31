using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Drop-in replacement for manually duplicating HexMapCanvasGraphic.
/// Place on a single GameObject with a RectTransform; it auto-creates
/// <see cref="ChunkCount"/> child graphics that share the same rect,
/// each drawing a subset of islands to stay under the 65k vertex limit.
/// </summary>
[ExecuteAlways]
public class HexMapComposite : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] HexagonTilemap source;

    [Header("Chunking")]
    [Min(1)]
    public int ChunkCount = 4;

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

    readonly List<HexMapCanvasGraphic> chunks = new();

    void OnEnable()
    {
        RebuildChunks();

        if (source)
        {
            source.OnMapChanged += OnMapChanged;
            if (source.currentHexMap != null)
                OnMapChanged(source.currentHexMap);
        }
    }

    void OnDisable()
    {
        if (source)
            source.OnMapChanged -= OnMapChanged;

        DestroyChunks();
    }

    void OnMapChanged(bool[,] map)
    {
        for (int i = 0; i < chunks.Count; i++)
            chunks[i].SetMap(map);
    }

    void RebuildChunks()
    {
        DestroyChunks();

        for (int i = 0; i < ChunkCount; i++)
        {
            var go = new GameObject($"HexChunk_{i}", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var graphic = go.AddComponent<HexMapCanvasGraphic>();
            graphic.IsManagedByComposite = true;
            graphic.ChunkIndex = i;
            graphic.ChunkCount = ChunkCount;
            graphic.DrawBackgroundInThisChunk = (i == 0);

            chunks.Add(graphic);
        }

        SyncSettings();
    }

    void DestroyChunks()
    {
        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            if (chunks[i] && chunks[i].gameObject)
            {
                if (Application.isPlaying)
                    Destroy(chunks[i].gameObject);
                else
                    DestroyImmediate(chunks[i].gameObject);
            }
        }
        chunks.Clear();
    }

    /// <summary>
    /// Push all visual settings from this composite to every chunk graphic.
    /// </summary>
    public void SyncSettings()
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            var g = chunks[i];
            if (!g) continue;

            g.HexSize = HexSize;

            g.SeaColor = SeaColor;
            g.LandColor = LandColor;

            g.CoastColor = CoastColor;
            g.MainWidth = MainWidth;
            g.MainWidthNoise = MainWidthNoise;
            g.MainNoiseFrequency = MainNoiseFrequency;
            g.DrawSecondaryStroke = DrawSecondaryStroke;
            g.SecondaryAlpha = SecondaryAlpha;
            g.SecondaryWidth = SecondaryWidth;
            g.SecondaryWidthNoise = SecondaryWidthNoise;
            g.SecondaryNoiseFrequency = SecondaryNoiseFrequency;
            g.SecondaryWobbleAmount = SecondaryWobbleAmount;
            g.SecondaryWobbleFrequency = SecondaryWobbleFrequency;

            g.WavesColor = WavesColor;
            g.WavesCount = WavesCount;
            g.WavesSpacing = WavesSpacing;
            g.WavesWidth = WavesWidth;
            g.WavesWidthNoise = WavesWidthNoise;
            g.WavesNoiseFrequency = WavesNoiseFrequency;

            g.MinIslandSize = MinIslandSize;
            g.SmoothingIterations = SmoothingIterations;
        }
    }

    /// <summary>
    /// Force every chunk to redraw with the current map data.
    /// </summary>
    public void RedrawAll()
    {
        if (source && source.currentHexMap != null)
            OnMapChanged(source.currentHexMap);
        else
        {
            for (int i = 0; i < chunks.Count; i++)
                chunks[i]?.SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (chunks.Count == 0)
            return;

        if (chunks.Count != ChunkCount)
            UnityEditor.EditorApplication.delayCall += () => { if (this) RebuildChunks(); };
        else
            SyncSettings();
    }
#endif
}
