Shader "Lit Surface"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [Toggle] _AlphaClip("Cutout", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        [NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax("Scale", Range(0.005, 0.08)) = 0.005
        [NoScaleOffset] _ParallaxMap("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        [NoScaleOffset] _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        [NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}

        _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

    SubShader
    {
        Pass
        {
            Name "Base Pass"

			Stencil
            {
                Ref 1
                Pass Replace
            }

            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
			ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
			#pragma target 5.0
            #pragma multi_compile_instancing
			#pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
            #include "LitSurface.hlsl"
            ENDHLSL
        }

        Pass
		{
			Name "Motion Vectors"
            Tags { "LightMode" = "MotionVectors" }

			Stencil
            {
                Ref 3
                Pass Replace
            }

			HLSLPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			#pragma target 5.0

			#pragma multi_compile_instancing
			#pragma multi_compile _ _ALPHATEST_ON

			#define MOTION_VECTORS_ON

			#include "LitSurface.hlsl"
			ENDHLSL
		}

        Pass
		{
			Colormask 0
            Cull [_Cull]
			ZClip[_ZClip]

            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
			#pragma multi_compile_instancing
            #pragma vertex Vertex
            #pragma fragment Fragment
			#pragma target 5.0
			#pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON
			#include "LitSurface.hlsl"
			ENDHLSL
		}
    }
}
