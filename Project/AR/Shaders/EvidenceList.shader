Shader "EtT/AR/EvidenceLit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Visibility("Visibility", Range(0,1)) = 1
        _Wetness("Wetness", Range(0,1)) = 0
        _Light("Light", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings   { float4 positionHCS:SV_POSITION; float3 normalWS: TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _Visibility;
                float  _Wetness;
                float  _Light;
                float  _Smoothness;
                float  _Metallic;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Prosty „lit” bez PBR – akcentujemy _Visibility i _Light
                float ndl = saturate(dot(normalize(IN.normalWS), _MainLightDirection.xyz * -1));
                float lit = lerp(0.2, 1.0, ndl) * lerp(0.3, 1.0, _Light);

                float3 col = _BaseColor.rgb * lit;
                col = lerp(col*0.2, col, _Visibility); // zanik

                // Wetness jako połysk pod światło (prostolinijnie, MVP)
                float spec = lerp(0.0, 0.35, _Wetness) * pow(ndl, 8.0);
                col += spec;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}