Shader "CustomShader/WaterShader"
{
    Properties
    {
        [Header(Textures)]
        _NormalTexture("Normal Texture", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 3)) = 0.5

        _NoiseTexture("Noise Texture", 2D) = "white" {}
        _VoronoiTexture("VoronoiTexture", 2D) = "white" {}

        [Header(Color)]
        _ShallowWaterColor("Shallow Water Color", Color) = (1, 1, 1, 1)
        _DeepWaterColor("Deep Water Color", Color) = (0, 0, 0, 0)
        _DeepWaterStrength("Deep Water Strength", Range(1, 10)) = 1

        [Header(Lighting)]
        _SmoothnessFersnelStrength("Smoothness Fersnel Strength", Range(0, 4)) = 1
        _SmoothnessFersnelMinValue("Smoothness Fersnel Min Value", Range(0, 1)) = 0.3
        _TransparencyFersnelStrength("Transparency Fersnel Strength", Range(0, 4)) = 1
        _TransparencyFersnelMinValue("Transparency Fersnel Min Value", Range(0, 1)) = 0.3

        [Header(Foam)]
        _NoiseTreshold("Noise Treshold", Range(0, 1)) = 0.5
        _FoamThiccness("FoamThicness", Range(0, 3)) = 1

        [Header(Animation)]
        _Speed("Speed", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags
        { 
            "RenderPipeline" = "UniversalRenderPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Pass {
            Name "ForwardLit"
            Tags{ "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma target 4.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX;
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "WaterShaderForwardLitPass.hlsl"
      
            ENDHLSL
        }
    }
}
