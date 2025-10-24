Shader "EtT/EvidenceLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
        _Visibility ("Visibility", Range(0,1)) = 1
        _Wetness ("Wetness", Range(0,1)) = 0
        _Light ("Light", Range(0,1)) = 1
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _BaseColor;
            half _Visibility, _Wetness, _Light, _Smoothness, _Metallic;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v){ v2f o; o.pos = TransformObjectToHClip(v.vertex.xyz); o.uv = v.uv; return o; }

            half4 frag(v2f i) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _BaseColor;
                // Widoczność: gasimy alpha i trochę kolor
                albedo.rgb *= lerp(0.3, 1.0, _Visibility);
                albedo.a   *= _Visibility;

                // „Oświetlenie” – uproszczone: ciemniej, gdy _Light niskie
                albedo.rgb *= lerp(0.4, 1.0, _Light);

                // „Mokre” – lekko rozjaśnij highlights
                albedo.rgb = lerp(albedo.rgb, saturate(albedo.rgb * 1.1 + 0.05), _Wetness);

                return albedo;
            }
            ENDHLSL
        }
    }
}