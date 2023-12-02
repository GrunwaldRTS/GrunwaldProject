#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

struct CustomLightingData{
	float3 positionWS;
	float3 meshNormalWS;
	float3 shapeNormalWS;
	float3 viewDirectionWS;
	float4 shadowCoord;

	float3 albedo;
	float smoothness;
	float ambientOcclusion;
	float3 subsurfaceColor;
	float thinness;
	float scatteringStrength;

	float3 bakedGI;
	float subsurfaceAmbientStrength;
};

float GetSmoothnessPower(float rawSmoothness){
	return exp2(10 * rawSmoothness + 1);
}

float3 CustomGlobalIllumination(CustomLightingData d){
	float3 indirectDiffuse = d.albedo * d.bakedGI * d.ambientOcclusion + 
		d.albedo * d.subsurfaceColor * (d.thinness * d.subsurfaceAmbientStrength);

	float3 reflectVector = reflect(-d.viewDirectionWS, d.meshNormalWS);
	float value = 1 - saturate(dot(d.viewDirectionWS, d.meshNormalWS));
	float fersnel = Pow4(value);
	float3 indirectSpecular = GlossyEnvironmentReflection(reflectVector,
		RoughnessToPerceptualRoughness(1 - d.smoothness),
		d.ambientOcclusion) * fersnel;

	return indirectDiffuse + indirectSpecular * d.smoothness;
}

float3 CustomLightHandling(CustomLightingData d, Light light){

	float3 radiance = light.color * (light.shadowAttenuation * light.distanceAttenuation);
	float3 tlucencyRadiance = radiance * d.subsurfaceColor;

	float diffuseMask = saturate(dot(d.shapeNormalWS, light.direction));
	
	float3 dirVec = normalize(d.viewDirectionWS + light.direction);
	float specularDot = saturate(dot(d.meshNormalWS, dirVec));
	float specularMask = pow(specularDot, GetSmoothnessPower(d.smoothness)) * diffuseMask;

	float3 scatteringDirection = normalize(-light.direction + d.meshNormalWS * d.scatteringStrength);
	float tlucencyDot = saturate(dot(d.viewDirectionWS, scatteringDirection));
	float tlucency = pow(tlucencyDot, GetSmoothnessPower(d.smoothness)) * d.thinness;

	float3 color = d.albedo * radiance * (diffuseMask + specularMask) +
		d.albedo * tlucencyRadiance * tlucency;

	return color;
}

float3 CalculateCustomLighting(CustomLightingData d){

	Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

	MixRealtimeAndBakedGI(mainLight, d.shapeNormalWS, d.bakedGI);
	float3 color = CustomGlobalIllumination(d);

	color += CustomLightHandling(d, mainLight);

	#ifdef _ADDITIONAL_LIGHTS

		uint numAdditionalLights = GetAdditionalLightsCount();
		for(uint i = 0; i < numAdditionalLights; i++)
		{
			Light light = GetAdditionalLight(i, d.positionWS, 1);
			color += CustomLightHandling(d, light);
		}

	#endif

	return color;
}

#endif

