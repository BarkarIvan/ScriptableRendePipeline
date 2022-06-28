Shader "Custom RP/Unlit"
{
    Properties {
        
        _MainTex ("Texture", 2D) = "white"{}
        _BaseColor ("BaseColor", Color) = (1,1,1,1)
        _CutOff ("CutOff", Range(0.0, 1.0)) = 0.5
       
         [Space]
        [Toggle(_ALPHACLIP)] _AlphaClip("Alpha clipping", Float) = 0
        
        [Space]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float)  = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Float) = 1
        }

    SubShader
    {
       
        Pass {
            
            Tags {"LightMode" = "CustomRP" }
            
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            
            
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma shader_feature _ALPHACLIP
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            
            #include "UnlitPass.hlsl"
            
            
            ENDHLSL
        }
    }

}


