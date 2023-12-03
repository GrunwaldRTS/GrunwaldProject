#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
	float2 uv : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

struct Interpolators{

	float4 positionCS : SV_POSITION;
	float3 positionWS : TEXCOORD0;

	float2 uv : TEXCOORD1;
	float2 normalUV : TEXCOORD2;
	float2 noiseUV : TEXCOORD3;
	float2 voronoiUV : TEXCOORD4;

	float3 normalWS : TEXCOORD5;
	float3 tangentWS : TEXCOORD6;
	float3 bitangentWS : TEXCOORD7;

	float fogFactor : TEXCOORD8;
	float4 screenPos : TEXCOORD9;

	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 10);
};

TEXTURE2D(_NormalTexture);
SAMPLER(sampler_NormalTexture);
float4 _NormalTexture_ST;

TEXTURE2D(_NoiseTexture);
SAMPLER(sampler_NoiseTexture);
float4 _NoiseTexture_ST;

TEXTURE2D(_VoronoiTexture);
SAMPLER(sampler_VoronoiTexture);
float4 _VoronoiTexture_ST;

float4 _CameraDepthTexture_TexelSize;

float _NormalStrength;

float4 _ShallowWaterColor;
float4 _DeepWaterColor;
float _DeepWaterStrength;

float _SmoothnessFersnelStrength;
float _SmoothnessFersnelMinValue;
float _TransparencyFersnelStrength;
float _TransparencyFersnelMinValue;

float _NoiseTreshold;
float _FoamThiccness;

float _Speed;

Interpolators Vertex(Attributes input){
	Interpolators output;

	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS.xyz);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	output.positionWS = posnInputs.positionWS;
	
	output.uv = input.uv;
	output.normalUV = TRANSFORM_TEX(input.uv, _NormalTexture);
	output.noiseUV = TRANSFORM_TEX(input.uv, _NoiseTexture);
	output.voronoiUV = TRANSFORM_TEX(input.uv, _VoronoiTexture);

	output.normalWS = normInputs.normalWS;
	output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
	output.bitangentWS = cross(output.normalWS, output.tangentWS);

	OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.normalWS, output.vertexSH);

	output.fogFactor = ComputeFogFactor(posnInputs.positionCS.z);
	output.screenPos = ComputeScreenPos(output.positionCS);

	return output;
}

float SampleDepthTexture(float4 screenPos){

	float2 screenUV = screenPos.xy / screenPos.w;
	float2 texelSize = _CameraDepthTexture_TexelSize.xy;

	float d1 = LinearEyeDepth(SampleSceneDepth(screenUV + float2(1.0, 0.0) * texelSize), _ZBufferParams).r;
	float d2 = LinearEyeDepth(SampleSceneDepth(screenUV + float2(-1.0, 0.0) * texelSize), _ZBufferParams).r;
	float d3 = LinearEyeDepth(SampleSceneDepth(screenUV + float2(0.0, 1.0) * texelSize), _ZBufferParams).r;
	float d4 = LinearEyeDepth(SampleSceneDepth(screenUV + float2(0.0, -1.0) * texelSize), _ZBufferParams).r;

	float depth = max(d1, max(d2, max(d3, d4))) - screenPos.w;
	
	return depth;
}

float3 GetNormalWS(float3 tangentWS, float3 bitangentWS, float3 normalWS, float3 normalTS){
	float3x3 mtxTangToWorld = {
		tangentWS.x, bitangentWS.x, normalWS.x,
		tangentWS.y, bitangentWS.y, normalWS.y,
		tangentWS.z, bitangentWS.z, normalWS.z
	};

	return mul(mtxTangToWorld, normalTS);
}

float2 ScrollScaleUV(float2 uv, float2 uvOffset, float2 uvScale, float speed){
	return (uv * uvScale) + uvOffset * (_Time.x * speed);
}

float3 GetScrolledNormalTS(float2 uv){
	float2 uv1 = ScrollScaleUV(uv, float2(1, 0.5), float2(1 ,1), _Speed);
	float2 uv2 = ScrollScaleUV(uv, float2(0.5, 1), float2(1 ,1), _Speed);

	float3 normal1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, uv1));
	float3 normal2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTexture, sampler_NormalTexture, uv2));

	return normal1 * normal2;
}

float3 NormalStrength(float3 In, float Strength)
{
    return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

float Remap(float value, float inMin, float inMax, float outMin, float outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}
float Remap(float3 value, float inMin, float inMax, float outMin, float outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}
float Remap(float3 value, float3 inMin, float3 inMax, float3 outMin, float3 outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}

float3 ApplyFoam(float3 albedo, float2 noiseUV, float2 voronoiUV, float depth){
	float2 noiseUV1 = ScrollScaleUV(noiseUV, float2(1, 1), float2(4, 1), _Speed);
	float2 noiseUV2 = ScrollScaleUV(noiseUV, float2(1, 1), float2(1, 2), _Speed);
	float2 noiseUV3 = ScrollScaleUV(noiseUV, float2(1, 1), float2(2, 2), _Speed);

	float3 noise1 = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV1);
	float3 noise2 = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV2);
	float3 noise3 = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV3);

	float2 voronoiUV1 = ScrollScaleUV(voronoiUV, float2(1, 1), float2(0.5, 0.5), _Speed);
	float2 voronoiUV2 = ScrollScaleUV(voronoiUV, float2(0.5, 0.5), float2(1, 1), _Speed);

	float3 voronoi1 = SAMPLE_TEXTURE2D(_VoronoiTexture, sampler_VoronoiTexture, voronoiUV1);
	float3 voronoi2 = SAMPLE_TEXTURE2D(_VoronoiTexture, sampler_VoronoiTexture, voronoiUV2);

	float3 voronoi = voronoi1 + voronoi2;
	voronoi = Remap(voronoi, 0, 2, 1, 0);

	float3 result = noise1 + noise2 + noise3;
	result = Remap(result, float3(0, 0, 0), float3(3, 3, 3), float3(0, 0, 0), float3(1, 1, 1));
	result *= voronoi;
	
	float mask = step(_NoiseTreshold * depth, result);

	//return result;
	return saturate(albedo + mask);
}

float GetAlpha(float3 viewDirectionWS, float3 normalWS, float strength, float minValue){
	float alphaDot = dot(viewDirectionWS, normalWS);
	alphaDot = Remap(alphaDot, 0, 1, 1, 0);
	alphaDot /= strength;
	alphaDot = saturate(alphaDot + minValue);

	return alphaDot;
}

float GetSmoothness(float3 viewDirectionWS, float3 normalWS, float strength, float minValue){
	float smoothnessDot = dot(normalWS, viewDirectionWS);
	smoothnessDot = Remap(smoothnessDot, 0, 1, 1, 0);
	smoothnessDot /= strength;
	smoothnessDot = saturate(smoothnessDot + minValue);

	return smoothnessDot;
}

float4 Fragment(Interpolators input) : SV_TARGET{
	float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);

	float3 smoothness = GetSmoothness(viewDirWS, input.normalWS, _SmoothnessFersnelStrength, _SmoothnessFersnelMinValue);
	// normalStrength = smoothness * smoothness * smoothness * _NormalStrength;

	float3 normalTS = NormalStrength(GetScrolledNormalTS(input.normalUV), _NormalStrength);
	float3 normalWS = GetNormalWS(input.tangentWS, input.bitangentWS, input.normalWS, normalTS);

	float depth = SampleDepthTexture(input.screenPos);
	float foamDepth = saturate(depth / _DeepWaterStrength);
	float edgeFoamDepth = saturate(depth / _FoamThiccness);

	float3 albedo = lerp(_ShallowWaterColor, _DeepWaterColor, foamDepth);
	albedo = ApplyFoam(albedo, input.noiseUV, input.voronoiUV, edgeFoamDepth);

	InputData lightingData = (InputData)0;
	lightingData.positionWS = input.positionWS;
	lightingData.normalWS = normalWS;
	lightingData.viewDirectionWS = viewDirWS;
	lightingData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
	lightingData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);

	SurfaceData surfaceInput;
	surfaceInput.albedo = albedo;
	surfaceInput.specular = 0;
	surfaceInput.metallic = 0;
	surfaceInput.smoothness = 1;
	surfaceInput.normalTS = 0;
	surfaceInput.emission = 0;
	surfaceInput.occlusion = 1;
	surfaceInput.alpha = GetAlpha(lightingData.viewDirectionWS, normalWS, _TransparencyFersnelStrength, _TransparencyFersnelMinValue);
	surfaceInput.clearCoatMask = 0;
	surfaceInput.clearCoatSmoothness = 0;

	float4 lightingOutput = UniversalFragmentPBR(lightingData, surfaceInput);

	return float4(MixFog(lightingOutput.rgb, input.fogFactor), lightingOutput.a);
}
