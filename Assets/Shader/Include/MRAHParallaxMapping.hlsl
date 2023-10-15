#ifndef Custom_Parallax_INCLUDE
#define Custom_Parallax_INCLUDE

void MRAH_ParallaxSample_float(float2 uv, float intensity, float3 viewDir, UnityTexture2D MRAH_Texture, UnitySamplerState sampleState, out float2 Out)
{
    const float minLayers = 30;
    const float maxLayers = 60;
    float numLayers = lerp(maxLayers, minLayers, abs(dot(float3(0, 0, 1), viewDir)));
    // float numLayers = minLayers;

    float numSteps = numLayers; //60.0f; // How many steps the UV ray tracing should take
    float height = 1.0;
    float step = 1.0 / numSteps;

    float2 offset = uv.xy;
    float HeightMap = MRAH_Texture.Sample(sampleState, offset).a;

    float2 delta = -viewDir.xy * intensity / (viewDir.z * numSteps);

    // find UV offset
    for (float i = 0.0f; i < numSteps; i++) 
    {
        if (HeightMap < height) 
        {
            height -= step;
            offset += delta;
            HeightMap = MRAH_Texture.Sample(sampleState, offset).a;
        } 
        else 
        {
            break;
        }
    }
    Out = offset; 
}  


#endif