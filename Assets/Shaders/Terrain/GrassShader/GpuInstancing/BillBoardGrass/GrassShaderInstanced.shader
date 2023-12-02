Shader "CustomShader/GrassShaderInstanced" {
    Properties{
        [Header(Transparency)]
        [MainTexture]_Transparency("TransparencyMap", 2D) = "white" {}
        _Cutoff("Alpha cutout threshold", Range(0, 1)) = 0.5

        [Header(Lighting)]
        _Smoothness("Smoothness", Range(0, 1)) = 0

        [Header(Color)]
        _TopColor("Top Color", Color) = (1, 1, 1, 1)
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 0)
        _MergeLine("Merge Line", Range(0, 3)) = 0.5

        [Header(Wind)]
        _Speed("Speed", Range(0, 10)) = 3
        _Scale("Scale", Range(0, 10)) = 1
    }
    SubShader{
        Tags{ 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "TransparentCutout"
        }
        Pass {
            Name "ShadowCaster"
            Tags{
                "LightMode" = "ShadowCaster"
            }

            Cull Off

            HLSLPROGRAM

            #pragma shader_feature_local _ALPHA_CUTOUT
            #pragma shader_feature_local _DOUBLE_SIDED_NORMALS

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "GrassShaderShadowCasterPassInstanced.hlsl"
            ENDHLSL
        }
        Pass {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Cull Off

            HLSLPROGRAM

            #define _SPECULAR_COLOR

            #pragma shader_feature_local _ALPHA_CUTOUT
            #pragma shader_feature_local _DOUBLE_SIDED_NORMALS

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "GrassShaderForwordLitPassInstanced.hlsl"
            ENDHLSL
        }
    }
}