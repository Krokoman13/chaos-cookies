using UnityEngine;

[ExecuteAlways]
public class WaterWaveBinder : MonoBehaviour
{
    [SerializeField] WaterWaveSettings settings;
    [SerializeField] Renderer waterRenderer;
    [SerializeField] Material waterMaterialOverride;

    static readonly int WaterLevelId = Shader.PropertyToID("_WaterLevel");
    static readonly int WaveAmplitudeId = Shader.PropertyToID("_WaveAmplitude");
    static readonly int WaveFrequencyId = Shader.PropertyToID("_WaveFrequency");
    static readonly int WaveSpeedId = Shader.PropertyToID("_WaveSpeed");

    static readonly int WaveDirection1Id = Shader.PropertyToID("_WaveDirection1");
    static readonly int WaveDirection2Id = Shader.PropertyToID("_WaveDirection2");

    static readonly int Wave2FrequencyMultiplierId = Shader.PropertyToID("_Wave2FrequencyMultiplier");
    static readonly int Wave2SpeedMultiplierId = Shader.PropertyToID("_Wave2SpeedMultiplier");
    static readonly int Wave2PhaseOffsetId = Shader.PropertyToID("_Wave2PhaseOffset");

    static readonly int Wave1WeightId = Shader.PropertyToID("_Wave1Weight");
    static readonly int Wave2WeightId = Shader.PropertyToID("_Wave2Weight");

    static readonly int FoamColorId = Shader.PropertyToID("_FoamColor");
    static readonly int FoamSizeId = Shader.PropertyToID("_FoamSize");
    static readonly int FoamDetailScaleId = Shader.PropertyToID("_FoamDetailScale");
    static readonly int FoamSpeedId = Shader.PropertyToID("_FoamSpeed");
    static readonly int FoamEvolutionSpeedId = Shader.PropertyToID("_FoamEvolutionSpeed");

    static readonly int FoamOpacityId = Shader.PropertyToID("_FoamOpacity");
    static readonly int FoamAmountId = Shader.PropertyToID("_FoamAmount");
    static readonly int FoamLineDensityId = Shader.PropertyToID("_FoamLineDensity");
    static readonly int FoamLineWidthId = Shader.PropertyToID("_FoamLineWidth");
    static readonly int FoamSoftnessId = Shader.PropertyToID("_FoamSoftness");
    static readonly int FoamBreakupId = Shader.PropertyToID("_FoamBreakup");
    static readonly int FoamCrestStrengthId = Shader.PropertyToID("_FoamCrestStrength");
    static readonly int FoamVoronoiWarp = Shader.PropertyToID("_FoamVoronoiWarp");


    static readonly int FoamUnderColorId = Shader.PropertyToID("_FoamUnderColor");
    static readonly int FoamUnderOpacityId = Shader.PropertyToID("_FoamUnderOpacity");
    static readonly int FoamUnderSizeMultiplier = Shader.PropertyToID("_FoamUnderSizeMultiplier");
    static readonly int FoamUnderLineWidthMultiplier = Shader.PropertyToID("_FoamUnderLineWidthMultiplier");
    static readonly int FoamUnderSoftnessMultiplayer = Shader.PropertyToID("_FoamUnderSoftnessMultiplayer");
    static readonly int FoamUnderOffset = Shader.PropertyToID("_FoamUnderOffset");


    Material TargetMaterial
    {
        get
        {
            if (waterMaterialOverride != null)
                return waterMaterialOverride;

            if (waterRenderer != null)
                return waterRenderer.sharedMaterial;

            return null;
        }
    }

    void Reset()
    {
        waterRenderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    void Update()
    {
        Apply();
    }

    public void Apply()
    {
        if (settings == null)
            return;

        Material material = TargetMaterial;

        if (material == null)
            return;

        material.SetFloat(WaterLevelId, settings.waterLevel);
        material.SetFloat(WaveAmplitudeId, settings.waveAmplitude);
        material.SetFloat(WaveFrequencyId, settings.waveFrequency);
        material.SetFloat(WaveSpeedId, settings.waveSpeed);

        material.SetVector(
            WaveDirection1Id,
            new Vector4(settings.waveDirection1.x, settings.waveDirection1.y, 0f, 0f)
        );

        material.SetVector(
            WaveDirection2Id,
            new Vector4(settings.waveDirection2.x, settings.waveDirection2.y, 0f, 0f)
        );

        material.SetFloat(Wave2FrequencyMultiplierId, settings.wave2FrequencyMultiplier);
        material.SetFloat(Wave2SpeedMultiplierId, settings.wave2SpeedMultiplier);
        material.SetFloat(Wave2PhaseOffsetId, settings.wave2PhaseOffset);

        material.SetFloat(Wave1WeightId, settings.wave1Weight);
        material.SetFloat(Wave2WeightId, settings.wave2Weight);

        material.SetColor(FoamColorId, settings.foamColor);
        material.SetFloat(FoamSizeId, settings.foamSize);
        material.SetFloat(FoamDetailScaleId, settings.foamDetailScale);
        material.SetFloat(FoamSpeedId, settings.foamSpeed);
        material.SetFloat(FoamEvolutionSpeedId, settings.foamEvolutionSpeed);
        material.SetFloat(FoamOpacityId, settings.foamOpacity);
        material.SetFloat(FoamAmountId, settings.foamAmount);
        material.SetFloat(FoamLineDensityId, settings.foamLineDensity);
        material.SetFloat(FoamLineWidthId, settings.foamLineWidth);
        material.SetFloat(FoamSoftnessId, settings.foamSoftness);
        material.SetFloat(FoamBreakupId, settings.foamBreakup);
        material.SetFloat(FoamCrestStrengthId, settings.foamCrestStrength);
        material.SetFloat(FoamCrestStrengthId, settings.foamCrestStrength);
        material.SetFloat(FoamVoronoiWarp, settings.foamVoronoiWarp);


        material.SetColor(FoamUnderColorId, settings.foamUnderColor);
        material.SetFloat(FoamUnderOpacityId, settings.foamUnderOpacity);
        material.SetFloat(FoamUnderSizeMultiplier, settings.foamUnderSizeMultiplier);
        material.SetFloat(FoamUnderLineWidthMultiplier, settings.foamUnderLineWidthMultiplier);
        material.SetFloat(FoamUnderSoftnessMultiplayer, settings.foamUnderSoftnessMultiplier);
        material.SetFloat(FoamUnderOffset, settings.foamUnderOffset);

    }
}