Shader "Hidden/Depth of Field"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #include "DepthOfField.hlsl"
            ENDHLSL
        }
    }
}