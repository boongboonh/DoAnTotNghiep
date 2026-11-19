Shader "Menu/Blossom Wind"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Mist ("Mist Mode", Range(0, 1)) = 0
        _FlowMask ("Flow Mask (R Sap, G Water)", 2D) = "black" {}
        _SapStrength ("Sap Glow", Range(0, 1)) = 0
        _WaterStrength ("Water Flow", Range(0, 1)) = 0
        _MotionTime ("Motion Time", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_FlowMask);
            SAMPLER(sampler_FlowMask);
            CBUFFER_START(UnityPerMaterial)
                float _Mist;
                float _MotionTime;
                float _SapStrength;
                float _WaterStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _MotionTime;
                float2 canopy = (uv - float2(0.51, 0.75)) / float2(0.30, 0.26);
                float crownMask = (1 - smoothstep(0.65, 1.05, length(canopy))) * smoothstep(0.57, 0.74, uv.y);
                float wind = sin(uv.x * 16 + uv.y * 9 - t * 0.77);
                float ripple = sin(uv.x * 29 - uv.y * 15 + t * 1.12);
                float2 crownMotion = crownMask * float2(0.0028 * wind, 0.0017 * ripple);
                float2 mistMotion = float2(
                    0.011 * sin(uv.y * 16 + uv.x * 7 - t * 0.28),
                    0.006 * sin(uv.x * 18 - t * 0.33) + 0.003 * sin(uv.x * 32 + t * 0.55));
                uv += lerp(crownMotion, mistMotion, _Mist);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(uv)) * input.color;
                float2 flowMask = SAMPLE_TEXTURE2D(_FlowMask, sampler_FlowMask, input.uv).rg;

                // A slow pulse travels upward along the painted veins.
                float sapPhase = frac(input.uv.y * 1.55 - t * 0.17);
                float sapPulse = smoothstep(0.73, 0.99, 1 - abs(sapPhase * 2 - 1));
                color.rgb += half3(1.0, 0.31, 0.076) * flowMask.r * (0.025 + 0.49 * sapPulse) * _SapStrength;

                // Long, faint strands descend inside the painted silhouette.
                float waterY = input.uv.y;
                float waterX = input.uv.x;
                float strand = 0.65 + 0.35 * sin(waterX * 480 + waterY * 8);
                float drift = sin(waterY * 36 + t * 0.74 + waterX * 173);
                float eddy = sin(waterY * 59 + t * 0.93 - waterX * 251);
                float shimmer = (0.64 + 0.23 * drift + 0.13 * eddy) * strand;
                float mistFade = smoothstep(0.12, 0.23, waterY);
                float water = flowMask.g * _WaterStrength * mistFade;
                color.rgb += half3(0.86, 0.91, 1.0) * water * 0.12 * shimmer;
                return color;
            }
            ENDHLSL
        }
    }
}
