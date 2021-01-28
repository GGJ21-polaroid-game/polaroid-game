Shader "Unlit/BillboardYConstrainedSprite"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

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

            v2f vert(appdata v)
            {
                v2f o;

                matrix mv = UNITY_MATRIX_MV;
                mv[0][0] = 1;
                mv[1][0] = 0;
                mv[2][0] = 0;
                mv[0][2] = 0;
                mv[1][2] = 0;
                mv[2][2] = 1;

                o.vertex = mul(UNITY_MATRIX_P, mul(mv, v.vertex));

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (col.a < 0.1)
                    discard;
                return col;
            }
            ENDCG
        }
    }
}
