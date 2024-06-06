Shader "CustomShader/MeshGrassShader" {
    Properties{
        _Scale("Scale", Range(0, 10)) = 1
        
        [Header(Lighting)]
        _Smoothness("Smoothness", Range(0, 1)) = 0

        [Header(Color)]
        _TopColor("Top Color", Color) = (1, 1, 1, 1)
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 0)
        _SubsurfaceColor("Subsurface Color", Color) = (1, 1, 1, 1)
        _Thinness("Thinness", Range(0, 1)) = 1
        _SubsurfaceAmbientStrength("SubsurfaceAmbientStrength", Range(0, 1)) = 1

        [Header(Wind)]
        _Droop("Droop", Range(-5, 5)) = 1
        _AnimationScale("AnimationScale", Range(0, 5)) = 1
    }
    SubShader{
        Tags{ 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque"
        }
        //Pass {
        //    Name "ShadowCaster"
        //    Tags{
        //        "LightMode" = "ShadowCaster"
        //    }

        //    Cull Off

        //    HLSLPROGRAM

        //    #pragma shader_feature_local _DOUBLE_SIDED_NORMALS

        //    #pragma vertex Vertex
        //    #pragma fragment Fragment

        //    #include "GeometryGrassShaderShadowCasterPass.hlsl"
        //    ENDHLSL
        //}
        Pass {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Cull Off

            HLSLPROGRAM

         

            #pragma shader_feature_local _DOUBLE_SIDED_NORMALS

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile_fog

            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "MeshGrassShaderForwardLitPass.hlsl"
            ENDHLSL
        }
    }
}