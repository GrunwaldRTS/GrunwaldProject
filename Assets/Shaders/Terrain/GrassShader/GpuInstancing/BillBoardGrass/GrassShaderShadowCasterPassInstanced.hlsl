#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "GrassShaderCommonInstanced.hlsl"

struct Mesh{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
};

struct Interpolators {
	float4 positionCS : SV_POSITION;
#ifdef _ALPHA_CUTOUT
	float2 uv : TEXCOORD0;
#endif
};

float3 _lightDirection;

float3 FlipNormalBasedOnViewDir(float3 normalWS, float3 positionWS){
	float3 viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
	return normalWS * dot(normalWS, viewDirWS) < 0? -1 : 1;
}

float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS){
	float3 lightDirectionWS = _lightDirection;
#ifdef _DOUBLE_SIDED_NORMALS
	normalWS = FlipNormalBasedOnViewDir(normalWS, positionWS);
#endif

	float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS ,lightDirectionWS));
#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#else
	positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
#endif
	return positionCS;
}

Interpolators Vertex(Mesh input, uint instanceID : SV_InstanceID){
	Interpolators output;

	float4 positionOS = mul(_Properties[instanceID].mat, input.positionOS);

	float3 windOffset = GetWindEffect(TransformObjectToWorld(positionOS), input.uv);

	VertexPositionInputs posnInputs = GetVertexPositionInputs(windOffset);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);
#ifdef _ALPHA_CUTOUT
	output.uv = input.uv;
#endif

	return output;
}

float4 Fragment(Interpolators input) : SV_TARGET{
#ifdef _ALPHA_CUTOUT
	float4 colorSample = SAMPLE_TEXTURE2D(_Transparency, sampler_Transparency, input.uv);
	TestAlphaClip(colorSample);
#endif

	return 0;
}