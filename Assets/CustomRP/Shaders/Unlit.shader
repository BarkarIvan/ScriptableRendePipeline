Shader "Custom RP/Unlit"
{
    Properties {}

    SubShader
    {
        Tags { }
        Pass {
            
            HLSLPROGRAM
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"





            
            ENDHLSL
        }
    }

}


