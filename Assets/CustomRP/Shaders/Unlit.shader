Shader "Custom RP/Unlit"
{
    Properties {
        
        _BaseColor ("BaseColor", Color) = (1,1,1,1)
        
        }

    SubShader
    {
       
        Pass {
            
             Tags {"LightMode" = "CustomRP" }
            
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            
            #include "UnlitPass.hlsl"
            
            
            ENDHLSL
        }
    }

}


