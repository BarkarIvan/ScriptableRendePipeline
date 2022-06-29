#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "ShaderLibrary/Core.hlsl"
#include "ShaderLibrary/Surface.hlsl"
#include "ShaderLibrary/Light.hlsl"
#include "ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord0 : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : NORMAL;
    float2 texcoord0 : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

TEXTURE2D(_MainTex);
SAMPLER (sampler_MainTex);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(half4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(half, _CutOff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)


Varyings LitPassVertex(Attributes i)
{
    Varyings o;
    o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
    o.normalWS = TransformObjectToWorldDir(i.normalOS);
    half4 mainTexST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    o.texcoord0 = mad(i.texcoord0, mainTexST.xy, mainTexST.zw);
    return o;
}


half4 LitPassFragment (Varyings i) : SV_Target
{
    half4 mainMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.texcoord0);
    Surface surface;
    surface.normal = normalize(i.normalWS);
    surface.color = mainMap * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    #ifdef _ALPHACLIP
    clip(mainMap.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CutOff));
    #endif
    surface.alpha = mainMap.a;
   
    
    half3 result = GetLighting(surface);
    return half4(result, surface.alpha);
}



#endif


