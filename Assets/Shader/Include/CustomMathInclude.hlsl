#ifndef Custom_Math_INCLUDE
#define Custom_Math_INCLUDE

void HeightLerp_float(float4 a, float4 b, float transition, float height, float fade, out float4 result)
{
    float t = smoothstep(height - fade, height + fade, transition);

    result =  lerp(a, b, t);
}

#endif