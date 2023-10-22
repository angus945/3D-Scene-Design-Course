#ifndef CUSTOM_LIGHTING_INCLUDED 
#define CUSTOM_LIGHTING_INCLUDED 

struct CustomLightingData
{
    float3 positionWS;
    float3 normalWS;
    float3 viewDirectionWS;
    float4 shadowCoord;
    
    float3 albedo;

    float fogFactor;
};

// https://www.youtube.com/watch?v=GQyCPaThQnA


#ifndef SHADERGRAPH_PREVIEW 
float3 CustomLightHandling(CustomLightingData d, Light light)
{
    //add shadow
    float3 radiance = light.color * (light.distanceAttenuation * light.shadowAttenuation) + half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);;
    
    float diffuse = saturate((dot(d.normalWS, light.direction) + 1) * 0.5);    
    float3 color = d.albedo * radiance * diffuse;

    return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d)
{
    #ifdef SHADERGRAPH_PREVIEW 
        float3 lightDir = float3(0.5, 0.5, 0);
        float intensity = saturate(dot(d.normalWS, lightDir));

        return d.albedo * intensity;
        
    #else
        //add shadow coord to light
        Light mainLight = GetMainLight(d.shadowCoord, d.positionWS, 1);

        float3 color = 0;
        color += CustomLightHandling(d, mainLight);

        #ifdef _ADDITIONAL_LIGHTS
            uint numAdditionalLights = GetAdditionalLightsCount();
            for (uint i = 0; i < numAdditionalLights; i++)
            {
                Light light = GetAdditionalLight(i, d.positionWS, 1);
                color += CustomLightHandling(d, light);
            }

        #endif
        
        color = MixFog(color, d.fogFactor);
        
        return color;
    #endif
    
}

void CalculateCustomLighting_float(float3 position, float3 normal, float3 viewDirection, float3 albedo, out float3 color)
{
    CustomLightingData d;
    d.positionWS = position;
    d.normalWS = normal;
    d.albedo = albedo;
    d.viewDirectionWS = viewDirection;

    #ifdef SHADERGRAPH_PREVIEW
        d.shadowCoord = 0;
        d.fogFactor = 0;
    #else
        //calculation shadow map
        float4 positionCS = TransformWorldToHClip(position);
        #if SHADOWS_SCREEN
            d.shadowCoord = ComputeScreenPos(positionCS);
        #else
            d.shadowCoord = TransformWorldToShadowCoord(position);
        #endif

        d.fogFactor = ComputeFogFactor(positionCS.z);
        
    #endif
    
    color = CalculateCustomLighting(d);
}

void CalculateMainLight_float(float3 worldPos, out float3 direction, out float3 color)
{
    #if SHADERGRAPH_PREVIEW
    direction = float3(0.5, 0.5, 0);
    color = 1;
    #else 
    Light mainLight = GetMainLight(0);
    direction = mainLight.direction;
    color = mainLight.color;
    #endif    
}

// https://youtu.be/GQyCPaThQnA


#endif