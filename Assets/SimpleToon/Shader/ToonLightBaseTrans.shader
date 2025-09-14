Shader "Lpk/LightModel/ToonLightBase_Transparent"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
        _ShadowStep("ShadowStep", Range(0, 1)) = 0.5
        _ShadowStepSmooth("ShadowStepSmooth", Range(0, 1)) = 0.04
        _SpecularStep("SpecularStep", Range(0, 1)) = 0.6
        _SpecularStepSmooth("SpecularStepSmooth", Range(0, 1)) = 0.05
        _SpecularColor("SpecularColor", Color) = (1,1,1,1)
        _RimStep("RimStep", Range(0, 1)) = 0.65
        _RimStepSmooth("RimStepSmooth",Range(0,1)) = 0.4
        _RimColor("RimColor", Color) = (1,1,1,1)
        _OutlineWidth("OutlineWidth", Range(0.0, 1.0)) = 0.15
        _OutlineColor("OutlineColor", Color) = (0.0, 0.0, 0.0, 1)
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            Pass
            {
                Name "UniversalForward"
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);

                CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _ShadowStep;
                float _ShadowStepSmooth;
                float _SpecularStep;
                float _SpecularStepSmooth;
                float4 _SpecularColor;
                float _RimStepSmooth;
                float _RimStep;
                float4 _RimColor;
                CBUFFER_END

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float2 uv : TEXCOORD0;
                    float4 normalWS : TEXCOORD1;
                    float3 viewDirWS : TEXCOORD4;
                    float4 positionCS : SV_POSITION;
                };

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                    output.positionCS = TransformWorldToHClip(posWS);
                    output.uv = input.uv;
                    float3 viewDirWS = GetCameraPositionWS() - posWS;
                    output.normalWS = float4(normalize(input.normalOS), 0);
                    output.viewDirWS = viewDirWS;
                    return output;
                }

                float4 frag(Varyings input) : SV_Target
                {
                    float3 N = normalize(input.normalWS.xyz);
                    float3 V = normalize(input.viewDirWS);
                    float3 L = normalize(_MainLightPosition.xyz);
                    float NL = dot(N,L) * 0.5 + 0.5;

                    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                    // diffuse + ambient + specular
                    float3 diffuse = _MainLightColor.rgb * baseMap.rgb * _BaseColor.rgb * NL;
                    float3 ambient = _RimColor.rgb; // 簡易
                    float3 finalColor = diffuse + ambient;

                    // アルファ値反映
                    float alpha = baseMap.a * _BaseColor.a;

                    return float4(finalColor, alpha);
                }
                ENDHLSL
            }
        }
}
