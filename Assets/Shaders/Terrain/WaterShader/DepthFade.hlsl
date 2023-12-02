#ifndef GRAYSCALE_INCLUDED
#define GRAYSCALE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D_X_FLOAT(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

void DepthFadeCustom_half(float4 screenPosition, out float Out){
    float2 texelSize = _CameraDepthTexture_TexelSize.xy;
    float2 screenUV = screenPosition.xy / screenPosition.w;                  
       
    float d1 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(1.0, 0.0) * texelSize).r;
    float d2 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(-1.0, 0.0) * texelSize).r;
    float d3 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(0.0, 1.0) * texelSize).r;
    float d4 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(0.0, -1.0) * texelSize).r;
 
    Out = min(d1, min(d2, min(d3, d4)));
}

void DepthFadeCustom_float(float4 screenPosition, out float Out){
    float2 texelSize = _CameraDepthTexture_TexelSize.xy;
    float2 screenUV = screenPosition.xy / screenPosition.w;                  
       
    float d1 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(1.0, 0.0) * texelSize).r;
    float d2 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(-1.0, 0.0) * texelSize).r;
    float d3 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(0.0, 1.0) * texelSize).r;
    float d4 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV + float2(0.0, -1.0) * texelSize).r;

    Out = min(d1, min(d2, min(d3, d4)));
}

#endif
