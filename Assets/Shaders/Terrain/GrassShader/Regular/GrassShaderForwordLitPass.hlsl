#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "GrassShaderCommon.hlsl"

struct Attributes {
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

struct Interpolators {

	float4 positionCS : SV_POSITION;

	float3 normalWS : TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	float2 uv : TEXCOORD2;
	float3 viewDirWS : TEXCOORD3;
	float fogFactor : TEXCOORD4;
	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
};

Interpolators Vertex(Attributes input) {
	Interpolators output;

	float3 windOffset = GetWindEffect(TransformObjectToWorld(input.positionOS), input.uv);

	VertexPositionInputs posnInputs = GetVertexPositionInputs(windOffset);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	output.positionWS = posnInputs.positionWS;
	output.viewDirWS = normalize(_WorldSpaceCameraPos - output.positionWS);
	output.normalWS = normInputs.normalWS;
	output.uv = input.uv;

	OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.normalWS, output.vertexSH);
	
	output.fogFactor = ComputeFogFactor(posnInputs.positionCS.z);

	return output;
}

float4 Fragment(Interpolators input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_TARGET {
	float4 colorSample = SAMPLE_TEXTURE2D(_Transparency, sampler_Transparency, input.uv);
	TestAlphaClip(colorSample);

	float3 normalWS = input.normalWS;
	normalWS *= IS_FRONT_VFACE(frontFace, 1, -1);

	float2 offsettedUv =  saturate(input.uv * 2 - float2(1, 0));
	float mask = distance(offsettedUv, float2(0, 0));
	float3 albedo = lerp(_BottomColor, _TopColor, mask);
	albedo *= _MergeLine;
	albedo = saturate(albedo);

	InputData lightingInput = (InputData)0;
	lightingInput.positionWS = input.positionWS;
	lightingInput.normalWS =  normalWS;
	lightingInput.viewDirectionWS = input.viewDirWS;
	lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
	lightingInput.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.normalWS);

	SurfaceData surfaceInput;
	surfaceInput.albedo = albedo;
	surfaceInput.specular = 0;
	surfaceInput.metallic = 0;
	surfaceInput.smoothness = _Smoothness;
	surfaceInput.normalTS = 0;
	surfaceInput.emission = 0;
	surfaceInput.occlusion = 1;
	surfaceInput.alpha = colorSample.a;
	surfaceInput.clearCoatMask = 0;
	surfaceInput.clearCoatSmoothness = 0;
	
	float4 lightningOutput = UniversalFragmentPBR(lightingInput, surfaceInput);

	return lerp(lightningOutput, float4(1,1,1,1), input.fogFactor);
}