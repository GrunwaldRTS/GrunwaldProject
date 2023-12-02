#ifndef GRASS_SHADER_COMMON_INCLUDED
#define GRASS_SHADER_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Random.hlsl"

float _Scale;

float4 _TopColor;
float4 _BottomColor;
float4 _SubsurfaceColor;
float _Thinness;
float _SubsurfaceAmbientStrength;

float _Smoothness;

float _Droop;
float _AnimationScale;

struct MeshProperties{
	float4 position;
	float2 worldUV;
};

StructuredBuffer<MeshProperties> _Properties;

#endif