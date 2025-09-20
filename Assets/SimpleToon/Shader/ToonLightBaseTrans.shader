Shader "Custom/TransparentDoubleSided"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,0.5)
        _MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha   // îºìßñæÉuÉåÉìÉh
        Cull Off                          // óºñ ï`âÊ
        ZWrite Off                        // îºìßñæÇÕê[ìxèëÇ´çûÇ›Çñ≥å¯âª

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texcol = tex2D(_MainTex, i.uv) * _Color;
                return texcol;
            }
            ENDCG
        }
    }
}
