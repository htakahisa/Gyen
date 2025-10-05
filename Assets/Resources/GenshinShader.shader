Shader "Custom/Outline_InvertedHull"
{
    Properties
    {
        _Color("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Float) = 0.05
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        // アウトライン用パス（裏面をカリングして押し出した面だけ描画する）
        Pass
        {
            Name "OUTLINE"
            Cull Front           // 内側を描かない（外側だけ見えるように）
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                // object spaceで法線方向に押し出す
                float3 n = normalize(v.normal);
                float3 pos = v.vertex.xyz + n * _OutlineWidth;
                o.pos = UnityObjectToClipPos(float4(pos,1));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }

        // オブジェクト本体の標準描画パスは通常のマテリアルで行ってください（別のマテリアルを使用）
    }
}
