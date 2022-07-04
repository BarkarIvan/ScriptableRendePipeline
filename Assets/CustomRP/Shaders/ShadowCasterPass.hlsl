#ifndef CUSTOM_SHADOWCASTER_PASS_INCLUDED
#define CUSTOM_SHADOWCASTER_PASS_INCLUDED

#include "ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 texcoord0 : TEXCOORD0;
};

struct  Varyings
{
    float3 positionCS : SV_POSITION;
    float2 texcoord0 : TEXCOORD0;
};


TEXTURE2D(_MainTex);
SAMPLER(sample_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(half4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(half, _CutOff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)


Varyings ShadowCasterPassVertex(Attributes i)
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i,o);
    o.positionCS = TransformObjectToHClip(i.positionOS);
    half4 mainTexST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    o.texcoord0 = mad(i.texcoord0, mainTexST.xy, mainTexST.zw);
    return o;
}

void ShadowCasterPassFragment(Varyings i )
{
    UNITY_SETUP_INSTANCE_ID(i);
    half4 mainMap = SAMPLE_TEXTURE2D(_MainTex, sample_MainTex, i.texcoord0);
    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    half4 result = mainMap * baseColor;

    #if defined(_ALPHACLIP)
        clip(result.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CutOff));
    #endif
}



#endif


