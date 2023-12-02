#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "GrassShaderCommon.hlsl"

struct Mesh{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
};

float3 _lightDirection;

float3 FlipNormalBasedOnViewDir(float3 normalWS, float3 positionWS){
	float3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
	return normalWS * dot(normalWS, viewDirWS) < 0? -1 : 1;
}

float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS){
	float3 lightDirectionWS = _lightDirection;

	normalWS = FlipNormalBasedOnViewDir(normalWS, positionWS);

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS ,lightDirectionWS));
#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif
	return positionCS;
}

Interpolators Vertex(Mesh input){
	Interpolators output;

	float3 windOffset = GetWindEffect(TransformObjectToWorld(input.positionOS), input.uv);

	VertexPositionInputs posnInputs = GetVertexPositionInputs(windOffset);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
	output.uv = input.uv;

	return output;
}

float4 Fragment(Interpolators input) : SV_TARGET{
	float4 colorSample = SAMPLE_TEXTURE2D(_Transparency, sampler_Transparency, input.uv);
	TestAlphaClip(colorSample);

	return 0;
}