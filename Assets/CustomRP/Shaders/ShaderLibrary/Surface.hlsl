#ifndef CUSTOM_SURFACE_INCLUDED
#define CUSTOM_SURFACE_INCLUDED


struct Surface
{
    float3 normal;
    float3 viewDirection;
    half3 color;
    half alpha;
    half metallic;
    half smoothness;
};

#endif


