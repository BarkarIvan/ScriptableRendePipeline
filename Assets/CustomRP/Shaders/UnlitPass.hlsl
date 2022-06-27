#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "ShaderLibrary/Common.hlsl"


half4 UnlitPassVertex(float3 positionOS : POSITION) : SV_POSITION
{
    return TransformObjectToHClip(positionOS);//
}

half4 UnlitPassFragment() : SV_TARGET
{
    return half4(1.0, 1.0, 0.0, 1.0);
}


#endif


