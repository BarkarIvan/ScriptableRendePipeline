#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#define MAX_SHADOWED_DIR_LIGHT_COUNT 4

TEXTURE2D_SHADOW(_DirShadowAtlas);
//#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(sampler_linear_clamp_compare); //SHADOW SAMPLER

CBUFFER_START(_CustomShadows)
float4x4 _DirShadowMatrices[MAX_SHADOWED_DIR_LIGHT_COUNT];
CBUFFER_END


struct DirShadowData
{
    half strength;
    int tileIndex;
};


half SampleDirShadowAtlas(float3 positionSTS) //shadow texture space
{
    return SAMPLE_TEXTURE2D_SHADOW(
        _DirShadowAtlas, sampler_linear_clamp_compare, positionSTS
        );
}


half GetDirShadowAttenuation(DirShadowData data, Surface surfaceWS)
{
    if(data.strength <= 0.0) //step???
    {
        return  1.0;
    }
    
    float3 positionSTS = mul(_DirShadowMatrices[data.tileIndex],
                            float4(surfaceWS.position,1.0)).xyz;
    half shadow = SampleDirShadowAtlas(positionSTS);
    
    return lerp(1.0, shadow, data.strength);
}
   

#endif
