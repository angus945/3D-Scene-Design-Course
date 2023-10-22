#ifndef TERRAIN_BLENDING_INCLUDE
#define TERRAIN_BLENDING_INCLUDE

uniform sampler2D _TerrainColorMap;
uniform sampler2D _TerrainHeightMap;
uniform float2 _BlendMapResoluction;

uniform float2 _TerrainSize;

uniform float _CameraHeight;
uniform float _CameraFarPlane;

void GetTerrainUV_float(float3 worldPos, out float2 terrainUV)
{
    terrainUV = worldPos.xz / _TerrainSize;
}
void GetTerrainColorMap_float(float2 uv, out float3 color)
{
    color = tex2D(_TerrainColorMap, uv);
}
void GetTerrainHeightMap_float(float2 uv, out float height)
{
    height = tex2D(_TerrainHeightMap, uv).x;
}
void GetTerrainBlendDistance_float(float3 worldPos, float heightMap, float fadeDistance, float fadeNoise, out float distance)
{
    float terrainWorldHeight = lerp(_CameraHeight - _CameraFarPlane, _CameraHeight, heightMap);
    distance = worldPos.y - terrainWorldHeight;
    // distance = distance - fadeNoise;
    distance = distance / fadeDistance;
    distance = distance + saturate(distance * fadeNoise);
    distance = saturate(distance);
}
void GetBlendingColor_float(float3 terrainColor, float3 objectColor, float distance, out float3 blendingColor)
{
    blendingColor = lerp(terrainColor, objectColor, distance);
}
void BlendWithTerrain_float(float3 worldPos, float3 objectColor, float fadeDistance, float fadeNoise, out float3 blendingColor)
{
    float2 uv;
    float3 terrainColor;
    float terrainHeight;
    float distance;

    GetTerrainUV_float(worldPos, uv);
    GetTerrainColorMap_float(uv, terrainColor);
    GetTerrainHeightMap_float(uv, terrainHeight);
    GetTerrainBlendDistance_float(worldPos, terrainHeight, fadeDistance, fadeNoise, distance);

    GetBlendingColor_float(terrainColor, objectColor, distance, blendingColor);
    // blendingColor = lerp(terrainColor, color, distance);
}

#endif