#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes{
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
};

struct Interpolators{
	float4 positionCS : SV_POSITION;
};

Interpolators Vertex(Attributes input){
	Interpolators output;

	VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS.zyx);

	output.positionCS = posnInputs.positionCS;

	return output;
}

float3 Fragment(Interpolators input) : SV_TARGET{
	return 0;
}
