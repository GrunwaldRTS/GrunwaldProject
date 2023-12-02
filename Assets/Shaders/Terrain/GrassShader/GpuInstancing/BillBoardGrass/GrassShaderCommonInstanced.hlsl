#ifndef GRASS_SHADER_COMMON_INCLUDED
#define GRASS_SHADER_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_Transparency); SAMPLER(sampler_Transparency);
float4 _Transparency_ST;
float _Cutoff;

float4 _TopColor;
float4 _BottomColor;
float _MergeLine;

float _Smoothness;

float _Speed;
float _Scale;

struct MeshProperties{
	float4x4 mat;
};

StructuredBuffer<MeshProperties> _Properties;

float4 GetPositionOS(float4x4 mat, float4 positionOS){
	return mul(mat, positionOS);
}

float3 GetWindEffect(float3 positionWS, float2 uv){
	float posFac = positionWS.x + positionWS.z;
	float sine = sin(posFac + _Time.y * _Speed) * 0.05 * _Scale;
	float sineMasked = uv.y * sine;
	float3 result = positionWS + float3(sineMasked, sineMasked, sineMasked);
	return TransformWorldToObject(result);
}

void TestAlphaClip(float4 colorSample){
	clip(colorSample.a - _Cutoff);
}

#endif