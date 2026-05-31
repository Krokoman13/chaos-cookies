using UnityEngine;

public static class WaterWaveMath
{
    public static float EvaluateHeight(Vector2 worldXZ, WaterWaveSettings settings, float time)
    {
        if (settings == null)
            return 0f;

        float wave1 = EvaluateSingleWave(
            worldXZ,
            settings.waveDirection1,
            settings.waveFrequency,
            settings.waveSpeed,
            0f,
            time
        );

        float wave2 = EvaluateSingleWave(
            worldXZ,
            settings.waveDirection2,
            settings.waveFrequency * settings.wave2FrequencyMultiplier,
            settings.waveSpeed * settings.wave2SpeedMultiplier,
            settings.wave2PhaseOffset,
            time
        );

        float combinedWave =
            wave1 * settings.wave1Weight +
            wave2 * settings.wave2Weight;

        return combinedWave * settings.waveAmplitude;
    }

    public static float EvaluateSurfaceY(Vector2 worldXZ, WaterWaveSettings settings, float time)
    {
        if (settings == null)
            return 0f;

        return settings.waterLevel + EvaluateHeight(worldXZ, settings, time);
    }

    public static Vector3 EvaluateNormal(
        Vector2 worldXZ,
        WaterWaveSettings settings,
        float time,
        float sampleDistance = 0.35f
    )
    {
        float left = EvaluateHeight(
            worldXZ + new Vector2(-sampleDistance, 0f),
            settings,
            time
        );

        float right = EvaluateHeight(
            worldXZ + new Vector2(sampleDistance, 0f),
            settings,
            time
        );

        float back = EvaluateHeight(
            worldXZ + new Vector2(0f, -sampleDistance),
            settings,
            time
        );

        float forward = EvaluateHeight(
            worldXZ + new Vector2(0f, sampleDistance),
            settings,
            time
        );

        Vector3 normal = new Vector3(
            left - right,
            sampleDistance * 2f,
            back - forward
        );

        return normal.normalized;
    }

    static float EvaluateSingleWave(
        Vector2 worldXZ,
        Vector2 direction,
        float frequency,
        float speed,
        float phaseOffset,
        float time
    )
    {
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        return Mathf.Sin(
            Vector2.Dot(worldXZ, direction) * frequency +
            time * speed +
            phaseOffset
        );
    }
}