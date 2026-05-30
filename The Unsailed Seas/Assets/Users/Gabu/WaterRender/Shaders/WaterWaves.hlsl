#ifndef WATER_WAVES_INCLUDED
#define WATER_WAVES_INCLUDED

float _WaterLevel;

float _WaveAmplitude;
float _WaveFrequency;
float _WaveSpeed;

float4 _WaveDirection1;
float4 _WaveDirection2;

float _Wave2FrequencyMultiplier;
float _Wave2SpeedMultiplier;
float _Wave2PhaseOffset;

float _Wave1Weight;
float _Wave2Weight;

// Foam controls
float _FoamSize;
float _FoamDetailScale;
float _FoamSpeed;
float _FoamEvolutionSpeed;
float _FoamOpacity;
float _FoamAmount;
float _FoamLineDensity;
float _FoamLineWidth;
float _FoamSoftness;
float _FoamBreakup;
float _FoamCrestStrength;
float _FoamVoronoiWarp;

// Dark under foam controls
float _FoamUnderOpacity;
float _FoamUnderSizeMultiplier;
float _FoamUnderLineWidthMultiplier;
float _FoamUnderSoftnessMultiplier;
float _FoamUnderOffset;

sampler2D _ShoreMaskTex;
float4 _ShoreMapOrigin;
float4 _ShoreMapSize;

float _ShoreFoamStrength;
float _ShoreWaveFrequency;
float _ShoreWaveSpeed;
float _ShoreWaveSharpness;
float _ShoreNoiseScale;
float _ShoreNoiseAmount;

float2 SafeNormalize2(float2 value)
{
    float lengthSquared = dot(value, value);

    if (lengthSquared < 0.0001)
        return float2(1, 0);

    return normalize(value);
}

float Hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float2 Hash22(float2 p)
{
    float n = Hash12(p);

    return float2(
        n,
        Hash12(p + n + 19.19)
    );
}

float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);

    float a = Hash12(i);
    float b = Hash12(i + float2(1, 0));
    float c = Hash12(i + float2(0, 1));
    float d = Hash12(i + float2(1, 1));

    float2 u = f * f * (3.0 - 2.0 * f);

    return lerp(
        lerp(a, b, u.x),
        lerp(c, d, u.x),
        u.y
    );
}

float FBM(float2 p)
{
    float value = 0.0;
    float amplitude = 0.5;

    value += ValueNoise(p) * amplitude;
    p *= 2.02;
    amplitude *= 0.5;

    value += ValueNoise(p) * amplitude;
    p *= 2.03;
    amplitude *= 0.5;

    value += ValueNoise(p) * amplitude;
    p *= 2.01;
    amplitude *= 0.5;

    value += ValueNoise(p) * amplitude;

    return value;
}
float EvaluateShoreWaveFoam(
    float2 worldXZ,
    float shoreMask,
    float time
)
{
    float shore = saturate(shoreMask);
    float distanceFromShore01 = 1.0 - shore;
    float shoreNoise = FBM(
        worldXZ * _ShoreNoiseScale
        + float2(time * 0.04, -time * 0.025)
        + 81.73
    );

    float phase =
        distanceFromShore01 * _ShoreWaveFrequency
        - time * _ShoreWaveSpeed
        + shoreNoise * _ShoreNoiseAmount;

    float wave = sin(phase) * 0.5 + 0.5;
    float waveLine = smoothstep(
        _ShoreWaveSharpness,
        1.0,
        wave
    );
    return waveLine * shore * _ShoreFoamStrength;
}

float EvaluateSingleWaterWave(
    float2 worldXZ,
    float2 direction,
    float frequency,
    float speed,
    float phaseOffset,
    float time
)
{
    direction = SafeNormalize2(direction);

    return sin(
        dot(worldXZ, direction) * frequency +
        time * speed +
        phaseOffset
    );
}

float EvaluateRawWaterWave(float2 worldXZ, float time)
{
    float wave1 = EvaluateSingleWaterWave(
        worldXZ,
        _WaveDirection1.xy,
        _WaveFrequency,
        _WaveSpeed,
        0,
        time
    );

    float wave2 = EvaluateSingleWaterWave(
        worldXZ,
        _WaveDirection2.xy,
        _WaveFrequency * _Wave2FrequencyMultiplier,
        _WaveSpeed * _Wave2SpeedMultiplier,
        _Wave2PhaseOffset,
        time
    );

    float weightSum = max(
        abs(_Wave1Weight) + abs(_Wave2Weight),
        0.0001
    );

    float combinedWave =
        wave1 * _Wave1Weight +
        wave2 * _Wave2Weight;

    combinedWave /= weightSum;

    return combinedWave;
}

float EvaluateWaterHeight(float2 worldXZ, float time)
{
    float wave = EvaluateRawWaterWave(worldXZ, time);

    // Slightly softens the extremes while keeping the same general wave shape.
    wave = wave - wave * wave * wave * 0.12;

    return wave * _WaveAmplitude;
}

float EvaluateWaterSurfaceY(float2 worldXZ, float time)
{
    return _WaterLevel + EvaluateWaterHeight(worldXZ, time);
}

float EvaluateWaterWave01(float2 worldXZ, float time)
{
    float height = EvaluateWaterHeight(worldXZ, time);

    float safeAmplitude = max(_WaveAmplitude, 0.0001);

    return saturate(height / safeAmplitude * 0.5 + 0.5);
}

float VoronoiEdge(float2 p)
{
    float2 cell = floor(p);
    float2 local = frac(p);

    float closest = 999.0;
    float secondClosest = 999.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 offset = float2(x, y);
            float2 randomPoint = Hash22(cell + offset);

            float2 pointPosition = offset + randomPoint - local;
            float distanceToPoint = length(pointPosition);

            if (distanceToPoint < closest)
            {
                secondClosest = closest;
                closest = distanceToPoint;
            }
            else if (distanceToPoint < secondClosest)
            {
                secondClosest = distanceToPoint;
            }
        }
    }

    return secondClosest - closest;
}
float2 WarpFoamUV(float2 p, float time)
{
    float evolveTime = time * _FoamEvolutionSpeed;

    float2 warp = float2(
        FBM(p * 0.65 + float2(evolveTime, evolveTime * 0.37) + 11.17),
        FBM(p * 0.65 + float2(-evolveTime * 0.41, evolveTime) + 47.83)
    );

    warp = warp * 2.0 - 1.0;

    return p + warp * _FoamVoronoiWarp;
}

float2 EvaluateWaterFoamLayers(float2 worldXZ, float time)
{
    float2 dir1 = SafeNormalize2(_WaveDirection1.xy);
    float2 dir2 = SafeNormalize2(_WaveDirection2.xy);

    float foamSize = max(_FoamSize, 0.0001);
    float foamDensity = max(_FoamLineDensity, 0.0001);
    float detailScale = max(_FoamDetailScale, 0.0001);

    float2 flowDirection = SafeNormalize2(
        dir1 * 0.7 + dir2 * 0.3
    );

    float2 underOffset = float2(
        _FoamUnderOffset,
        _FoamUnderOffset * 1.37
    );

    float2 topUV =
        worldXZ / foamSize * foamDensity
        + flowDirection * time * _FoamSpeed;

    float2 underUV =
        worldXZ / (foamSize * max(_FoamUnderSizeMultiplier, 0.0001)) * foamDensity
        + flowDirection * time * _FoamSpeed * 0.92
        + underOffset;

    float topEdge = VoronoiEdge(WarpFoamUV(topUV, time));
    float underEdge = VoronoiEdge(WarpFoamUV(underUV + 13.37, time));

    float topLines = 1.0 - smoothstep(
        _FoamLineWidth,
        _FoamLineWidth + _FoamSoftness,
        topEdge
    );

    float underLineWidth =
        _FoamLineWidth * max(_FoamUnderLineWidthMultiplier, 0.0001);

    float underSoftness =
        _FoamSoftness * max(_FoamUnderSoftnessMultiplier, 0.0001);

    float underLines = 1.0 - smoothstep(
        underLineWidth,
        underLineWidth + underSoftness,
        underEdge
    );

    float2 detailUV =
        worldXZ * detailScale
        - dir2 * time * _FoamSpeed * 0.5;

    float breakupNoise = 1.0f;// FBM(detailUV + 31.27);

    float topBreakup = smoothstep(
        _FoamBreakup,
        1.0,
        breakupNoise
    );

    float underBreakup = smoothstep(
        saturate(_FoamBreakup - 0.2),
        1.0,
        breakupNoise
    );

    float wave01 = EvaluateWaterWave01(worldXZ, time);

    float crestFoam = smoothstep(
        0.65,
        1.0,
        wave01
    ) * _FoamCrestStrength;

    float underFoam =
        underLines * underBreakup * _FoamAmount
        + crestFoam * 0.65;

    float topFoam =
        topLines * topBreakup * _FoamAmount
        + crestFoam;

    underFoam = saturate(underFoam) * _FoamUnderOpacity;
    topFoam = saturate(topFoam) * _FoamOpacity;

    return float2(underFoam, topFoam);
}

// Optional compatibility function.
// If old shader code still calls EvaluateWaterFoam(), it will get only the white top layer.
float EvaluateWaterFoam(float2 worldXZ, float time)
{
    return EvaluateWaterFoamLayers(worldXZ, time).y;
}



#endif