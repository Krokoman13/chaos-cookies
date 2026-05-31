Shader "Custom/Water/SimpleWaveDepthWater"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.25, 0.85, 0.9, 1)
        _DeepColor ("Deep Color", Color) = (0.02, 0.18, 0.45, 1)

        _DepthMaxDistance ("Depth Fog Distance", Float) = 6

        _AlphaShallow ("Alpha Shallow", Range(0, 1)) = 0.35
        _AlphaDeep ("Alpha Deep", Range(0, 1)) = 0.75

        _WaterLevel ("Water Level", Float) = 0

        _WaveAmplitude ("Wave Amplitude", Float) = 0.12
        _WaveFrequency ("Wave Frequency", Float) = 1.4
        _WaveSpeed ("Wave Speed", Float) = 0.8

        _WaveDirection1 ("Wave Direction 1", Vector) = (1, 0.25, 0, 0)
        _WaveDirection2 ("Wave Direction 2", Vector) = (-0.4, 1, 0, 0)

        _Wave2FrequencyMultiplier ("Wave 2 Frequency Multiplier", Float) = 1.7
        _Wave2SpeedMultiplier ("Wave 2 Speed Multiplier", Float) = 0.75
        _Wave2PhaseOffset ("Wave 2 Phase Offset", Float) = 1.8

        _Wave1Weight ("Wave 1 Weight", Float) = 1
        _Wave2Weight ("Wave 2 Weight", Float) = 0.65

        _SurfaceLight ("Surface Light", Range(0, 1)) = 0.12

        [Header(Foam Top Layer)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamSize ("Foam Size", Float) = 4
        _FoamDetailScale ("Foam Detail Scale", Float) = 3
        _FoamSpeed ("Foam Speed", Float) = 0.08
        _FoamEvolutionSpeed ("Foam Evolution Speed", Float) = 0.08
        _FoamOpacity ("Foam Opacity", Range(0, 1)) = 0.2
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.65
        _FoamLineDensity ("Foam Cell Density", Float) = 1.5
        _FoamLineWidth ("Foam Line Width", Range(0, 1)) = 0.045
        _FoamSoftness ("Foam Softness", Range(0, 1)) = 0.035
        _FoamBreakup ("Foam Breakup", Range(0, 1)) = 0.35
        _FoamCrestStrength ("Foam Crest Strength", Range(0, 1)) = 0.04
        _FoamVoronoiWarp ("Foam Roundness", Range(0,1)) = 0.30

        [Header(Foam Under Layer)]
        _FoamUnderColor ("Foam Under Color", Color) = (0.03, 0.32, 0.42, 1)
        _FoamUnderOpacity ("Foam Under Opacity", Range(0, 1)) = 0.18
        _FoamUnderSizeMultiplier ("Foam Under Size Multiplier", Float) = 1.05
        _FoamUnderLineWidthMultiplier ("Foam Under Line Width Multiplier", Float) = 2.2
        _FoamUnderSoftnessMultiplier ("Foam Under Softness Multiplier", Float) = 1.8
        _FoamUnderOffset ("Foam Under Offset", Float) = 0.17

        [Header(Shore Foam)]
        _ShoreMaskTex ("Shore Mask Texture", 2D) = "black" {}
        _ShoreMapOrigin ("Shore Map Origin", Vector) = (0, 0, 0, 0)
        _ShoreMapSize ("Shore Map Size", Vector) = (100, 100, 0, 0)

        _ShoreFoamStrength ("Shore Foam Strength", Range(0, 1)) = 0.8
        _ShoreWaveFrequency ("Shore Wave Frequency", Float) = 18
        _ShoreWaveSpeed ("Shore Wave Speed", Float) = 1.2
        _ShoreWaveSharpness ("Shore Wave Sharpness", Range(0, 1)) = 0.65
        _ShoreNoiseScale ("Shore Noise Scale", Float) = 0.08
        _ShoreNoiseAmount ("Shore Noise Amount", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "WaterWaves.hlsl"

            sampler2D _CameraDepthTexture;

            fixed4 _ShallowColor;
            fixed4 _DeepColor;

            fixed4 _FoamColor;
            fixed4 _FoamUnderColor;

            float _DepthMaxDistance;

            float _AlphaShallow;
            float _AlphaDeep;

            float _SurfaceLight;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
                float waveValue : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;

                worldPosition.y = EvaluateWaterSurfaceY(
                    worldPosition.xz,
                    _Time.y
                );

                o.position = UnityWorldToClipPos(worldPosition);
                o.screenPosition = ComputeScreenPos(o.position);

                COMPUTE_EYEDEPTH(o.screenPosition.z);

                o.worldPosition = worldPosition;

                o.waveValue = EvaluateWaterWave01(
                    worldPosition.xz,
                    _Time.y
                );

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 shoreUV =
                    (i.worldPosition.xz - _ShoreMapOrigin.xy)
                    / max(_ShoreMapSize.xy, 0.0001);

                float shoreMask = tex2D(_ShoreMaskTex, shoreUV).r;
                float rawSceneDepth = SAMPLE_DEPTH_TEXTURE_PROJ(
                    _CameraDepthTexture,
                    UNITY_PROJ_COORD(i.screenPosition)
                );

                float sceneEyeDepth = LinearEyeDepth(rawSceneDepth);
                float waterEyeDepth = i.screenPosition.z;

                float waterDepth = sceneEyeDepth - waterEyeDepth;
                waterDepth = max(0, waterDepth);

                float depth01 = saturate(
                    waterDepth / max(0.0001, _DepthMaxDistance)
                );

                float3 waterColor = lerp(
                    _ShallowColor.rgb,
                    _DeepColor.rgb,
                    depth01
                );

                float waveHighlight = smoothstep(
                    0.5,
                    1.0,
                    i.waveValue
                );

                waterColor += waveHighlight * _SurfaceLight;

                float2 foamLayers = EvaluateWaterFoamLayers(
                    i.worldPosition.xz,
                    _Time.y
                );

                float shoreFoam = EvaluateShoreWaveFoam(
                    i.worldPosition.xz,
                    shoreMask,
                    _Time.y
                );

                float underFoam = saturate(foamLayers.x + shoreFoam * 0.35);
                float topFoam = saturate(foamLayers.y + shoreFoam);

                waterColor = lerp(
                    waterColor,
                    _FoamUnderColor.rgb,
                    underFoam
                );

                waterColor = lerp(
                    waterColor,
                    _FoamColor.rgb,
                    topFoam
                );

                float alpha = lerp(
                    _AlphaShallow,
                    _AlphaDeep,
                    depth01
                );

                alpha = saturate(
                    alpha
                    + underFoam * 0.04
                    + topFoam * 0.08
                );

                return fixed4(
                    saturate(waterColor),
                    alpha
                );
            }

            ENDCG
        }
    }
}