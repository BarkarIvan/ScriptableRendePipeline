#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 texcoord0 : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 texcoord0 : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
   UNITY_DEFINE_INSTANCED_PROP(half4, _MainTex_ST)
   UNITY_DEFINE_INSTANCED_PROP( half4, _BaseColor)
   UNITY_DEFINE_INSTANCED_PROP( half, _CutOff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

Varyings UnlitPassVertex(Attributes i) 
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(i);
    UNITY_TRANSFER_INSTANCE_ID(i,o);

    o.positionCS = TransformObjectToHClip(i.positionOS);

    half4 mainTexST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    o.texcoord0 = i.texcoord0 * mainTexST.xy + mainTexST.zw;

    return o;
}

half4 UnlitPassFragment(Varyings i) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i);
    half4 mainMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord0);
    half4 color = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    half cutOff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CutOff);

    #ifdef _ALPHACLIP
     clip(mainMap.a - cutOff);
    #endif

    return mainMap * color;
}

#endif


