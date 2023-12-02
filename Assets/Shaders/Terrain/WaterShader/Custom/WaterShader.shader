Shader "CustomShader/WaterShader"
{
    Properties
    {
        _ShallowWaterColor("Shallow Water Color", Color) = (1, 1, 1, 1)
        _DeepWaterColor("Deep Water Color", Color) = (0, 0, 0, 0)
        _DeepWaterStrength("Deep Water Strength", Range(1, 5)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }
        Pass {
            Name "ForwardLit"
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "WaterShaderForwardLitPass.hlsl"
      
            ENDHLSL
        }
    }
}
