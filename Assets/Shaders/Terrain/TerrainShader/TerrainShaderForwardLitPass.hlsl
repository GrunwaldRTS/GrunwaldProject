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
	float3 normalWS : TEXCOORD2;
	float fogFactor : TEXCOORD3;

	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 4);
};

struct Height{
	float startHeight;
	float endHeight;
	float4 albedo;
};

int _HeightsCount;
StructuredBuffer<Height> _Heights;

Interpolators Vertex(Attributes input){
	Interpolators output;

	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS.xyz);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	output.positionWS = posnInputs.positionWS;
	
	output.uv = input.uv;
	output.normalWS = normInputs.normalWS;

	OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.normalWS, output.vertexSH);

	output.fogFactor = ComputeFogFactor(posnInputs.positionCS.z);

	return output;
}

float Remap(float value, float inMin, float inMax, float outMin, float outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}
float Remap(float3 value, float inMin, float inMax, float outMin, float outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}
float4 Remap(float4 value, float4 inMin, float4 inMax, float4 outMin, float4 outMax){
	return outMin + (outMax - outMin) * ((value - inMin) / (inMax - inMin));
}

float4 Fragment(Interpolators input) : SV_TARGET{
	float index = 0;

	for(int i = 1; i < _HeightsCount; i++){
		index = input.positionWS.y > _Heights[i].startHeight? i : index;
	}

	float3 albedo = _Heights[index].albedo;

	if(index + 1 < _HeightsCount){
		float height = saturate(Remap(input.positionWS.y, _Heights[index].startHeight, _Heights[index].endHeight, 0, 1));
		albedo = lerp(albedo, _Heights[index + 1].albedo, height);
	}

	InputData lightingData = (InputData)0;
	lightingData.positionWS = input.positionWS;
	lightingData.normalWS = input.normalWS;
	lightingData.viewDirectionWS = normalize(_WorldSpaceCameraPos - input.positionWS);;
	lightingData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
	lightingData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.normalWS);

	SurfaceData surfaceInput;
	surfaceInput.albedo = albedo;
	surfaceInput.specular = 0;
	surfaceInput.metallic = 0;
	surfaceInput.smoothness = 0;
	surfaceInput.normalTS = 0;
	surfaceInput.emission = 0;
	surfaceInput.occlusion = 1;
	surfaceInput.alpha = 1;
	surfaceInput.clearCoatMask = 0;
	surfaceInput.clearCoatSmoothness = 0;

	float4 lightingOutput = UniversalFragmentPBR(lightingData, surfaceInput);

	return float4(MixFog(lightingOutput.rgb, input.fogFactor), 1);
}
