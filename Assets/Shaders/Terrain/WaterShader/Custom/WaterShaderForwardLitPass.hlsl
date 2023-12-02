#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

struct Interpolators{
	float4 positionCS : SV_POSITION;
	float3 positionWS : TEXCOORD0;
	float3 normalWS : TEXCOORD1;
	float2 uv : TEXCOORD2;
	float3 viewDirWS : TEXCOORD3;
	float fogFactor : TEXCOORD4;
	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 5);
};

sampler2D _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;

float4 _ShallowWaterColor;
float4 _DeepWaterColor;
float _DeepWaterStrength;
float _Smoothness;

Interpolators Vertex(Attributes input){
	Interpolators output;

	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS.xyz);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	output.positionWS = posnInputs.positionWS;
	output.normalWS = normInputs.normalWS;
	output.uv = input.uv;
	output.viewDirWS = normalize(_WorldSpaceCameraPos - output.positionWS);

	OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.normalWS, output.vertexSH);

	output.fogFactor = ComputeFogFactor(posnInputs.positionCS.z);

	return output;
}

float4 Fragment(Interpolators input) : SV_TARGET{

	float3 normalWS = normalize(input.normalWS);
	float3 albedo = float3(0, 0, 0);

	InputData lightingData = (InputData)0;
	lightingData.positionWS = input.positionWS;
	lightingData.normalWS = normalWS;
	lightingData.viewDirectionWS = input.viewDirWS;
	lightingData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
	lightingData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, normalWS);

	SurfaceData surfaceInput;
	surfaceInput.albedo = albedo;
	surfaceInput.specular = 0;
	surfaceInput.metallic = 0;
	surfaceInput.smoothness = _Smoothness;
	surfaceInput.normalTS = 0;
	surfaceInput.emission = 0;
	surfaceInput.occlusion = 1;
	surfaceInput.alpha = 1;
	surfaceInput.clearCoatMask = 0;
	surfaceInput.clearCoatSmoothness = 0;

	float4 lightingOutput = UniversalFragmentPBR(lightingData, surfaceInput);

	return float4(MixFog(lightingOutput.rgb, input.fogFactor), 1);
}
