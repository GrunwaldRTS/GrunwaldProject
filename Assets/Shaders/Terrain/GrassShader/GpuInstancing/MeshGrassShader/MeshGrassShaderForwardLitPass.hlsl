#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "MeshGrassShaderCommon.hlsl"
#include "CustomLighting.hlsl"
#include "Rotation.hlsl"

struct Attributes {
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

struct Interpolators {

	float4 positionCS : SV_POSITION;

	float3 meshNormalWS : TEXCOORD0;
	float3 meshBiTangentWS : TEXCOORD1;
	float3 shapeNormalWS : TEXCOORD2;
	float3 positionWS : TEXCOORD3;
	float2 uv : TEXCOORD4;
	float fogFactor : TEXCOORD6;
	DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 7);
	float2 worldUV : TEXCOORD9;
};

float4 _Albedo;
sampler2D _WindTex;

float4 CalculateLocalPos(uint instanceID, float4 meshLocalPos, float2 meshUv){
	float4 grassPosition = _Properties[instanceID].position;

	float4 localpos = RotateAroundXInDegrees(meshLocalPos, 90.0f);

	float idHash = randValue(abs(grassPosition.x * 10000 + grassPosition.y * 100 + grassPosition.z * 0.05f + 2));
    idHash = randValue(idHash * 100000);

    float4 animationDirection = float4(0.0f, 0.0f, 1.0f, 0.0f);
    animationDirection = normalize(RotateAroundYInDegrees(animationDirection, idHash * 180.0f));

	localpos = RotateAroundYInDegrees(localpos, idHash * 180.0f);
	localpos.y += _Scale * meshUv.y * meshUv.y * meshUv.y;
    localpos.xz += _Droop * lerp(0.5f, 1.0f, idHash) * (meshUv.y * meshUv.y * _Scale) * animationDirection;

	float4 worldUV = float4(_Properties[instanceID].worldUV, 0, 0);
                
	float4 wind = tex2Dlod(_WindTex, worldUV) * _AnimationScale;

    float swayVariance = lerp(0.8, 1.0, idHash);
    float movement = meshUv.y * meshUv.y * (wind.r);
    movement *= swayVariance;
                
    localpos.xz += movement;

	return float4(grassPosition.xyz + localpos.xyz, 1.0f);
}

Interpolators Vertex(Attributes input, uint instanceID : SV_InstanceID) {
	Interpolators output;

	float4 localpos = CalculateLocalPos(instanceID, input.positionOS, input.uv);

	VertexPositionInputs posnInputs = GetVertexPositionInputs(localpos);
	VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);

	output.positionCS = posnInputs.positionCS;
	output.positionWS = posnInputs.positionWS;
	output.meshNormalWS = normInputs.normalWS;
	output.meshBiTangentWS = normInputs.bitangentWS;
	output.uv = input.uv;

	output.worldUV = _Properties[instanceID].worldUV;

	OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.meshBiTangentWS, output.vertexSH);

	output.fogFactor = ComputeFogFactor(posnInputs.positionCS.z);

	return output;
}

float3 Fragment(Interpolators input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_TARGET {
	float3 meshNormalWS = input.meshNormalWS;
	meshNormalWS *= IS_FRONT_VFACE(frontFace, 1, -1);

	float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);

	float3 albedo = lerp(_BottomColor, _TopColor, input.uv.y);
	//albedo = float3(input.worldUV.r, input.worldUV.g, 0);
	//albedo = _Albedo;

	CustomLightingData lightingData = (CustomLightingData)0;

	lightingData.positionWS = input.positionWS;
	lightingData.meshNormalWS = meshNormalWS;
	lightingData.shapeNormalWS = input.meshBiTangentWS;
	lightingData.viewDirectionWS = viewDirWS;
	lightingData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

	lightingData.albedo = albedo;
	lightingData.smoothness = _Smoothness;
	lightingData.ambientOcclusion = 1;
	lightingData.subsurfaceColor = _SubsurfaceColor;
	lightingData.thinness = _Thinness;

	lightingData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.meshBiTangentWS);
	lightingData.subsurfaceAmbientStrength = _SubsurfaceAmbientStrength;

	float3 lightingOutput = CalculateCustomLighting(lightingData);

	return lightingOutput;
}