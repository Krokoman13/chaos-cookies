using UnityEngine;

[CreateAssetMenu(menuName = "Water/Water Wave Settings")]
public class WaterWaveSettings : ScriptableObject
{
    [Header("Water Level")]
    public float waterLevel = 0f;

    [Header("Wave Shape")]
    public float waveAmplitude = 0.12f;
    public float waveFrequency = 1.4f;
    public float waveSpeed = 0.8f;

    public Vector2 waveDirection1 = new Vector2(1f, 0.25f);
    public Vector2 waveDirection2 = new Vector2(-0.4f, 1f);

    [Header("Second Wave")]
    public float wave2FrequencyMultiplier = 1.75f;
    public float wave2SpeedMultiplier = 1.35f;
    public float wave2PhaseOffset = 1.8f;

    [Header("Blending")]
    [Range(0f, 1f)] public float wave1Weight = 0.65f;
    [Range(0f, 1f)] public float wave2Weight = 0.35f;

    [Header("Foam")]
    public Color foamColor = Color.white;

    public float foamSize = 4f;
    public float foamDetailScale = 5f;
    public float foamSpeed = 0.08f;
    public float foamEvolutionSpeed = 1f;

    [Range(0f, 1f)] public float foamOpacity = 0.18f;
    [Range(0f, 1f)] public float foamAmount = 0.45f;

    public float foamLineDensity = 5f;

    [Range(0f, 1f)] public float foamLineWidth = 0.1f;
    [Range(0f, 1f)] public float foamSoftness = 0.08f;
    [Range(0f, 1f)] public float foamBreakup = 0.6f;
    [Range(0f, 1f)] public float foamCrestStrength = 0.05f;
    [Range(0f, 1f)] public float foamVoronoiWarp = 0.30f;


    public Color foamUnderColor = new Color(0.03f, 0.32f, 0.42f, 1);
    [Range(0f, 1f)] public float foamUnderOpacity = 0.18f;
    [Range(0f, 1f)] public float foamUnderSizeMultiplier = 1.05f;
    [Range(0f, 1f)] public float foamUnderLineWidthMultiplier = 2.2f;
    [Range(0f, 1f)] public float foamUnderSoftnessMultiplier = 1.8f;
    [Range(0f, 1f)] public float foamUnderOffset = 0.17f;

}   