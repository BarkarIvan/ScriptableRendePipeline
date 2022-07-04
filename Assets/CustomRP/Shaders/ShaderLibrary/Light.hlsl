#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    half4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END


struct Light
{
    half3 color;
    float3 direction;
    half attenuation;
};

DirShadowData GetDirectionalShadowData(int lightIndex)
{
    DirShadowData data;
    data.strength = _DirLightShadowData[lightIndex].x;
    data.tileIndex = _DirLightShadowData[lightIndex].y;
    return  data;
}



int GetDirectionalLightCount()
{
    return  _DirectionalLightCount;
}

Light GetDirectionalLight(int index, Surface surfaceWS)
{
    Light light;
    light.color = _DirectionalLightColors[index].rgb;
    light.direction = _DirectionalLightDirections[index].xyz;
    DirShadowData shadowData = GetDirectionalShadowData(index);
    light.attenuation = GetDirShadowAttenuation(shadowData, surfaceWS);
    return light;
}



#endif


