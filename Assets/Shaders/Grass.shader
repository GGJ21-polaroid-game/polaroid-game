Shader "Unlit/Grass"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        _TessellationDistanceMin("Tessellation Distance Min", float) = 1
        _TessellationDistanceMax("Tessellation Distance Max", float) = 1
        _TessellationFactorMin("Tessellation Factor Min", Range(1, 50)) = 1
        _TessellationFactorMax("Tessellation Factor Max", Range(1, 50)) = 1

        _GrassHeight("Grass height", float) = 1
        _GrassWidth("Grass width", float) = 1
        _GrassOffset("Grass random offset", float) = 1

        _WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
        _WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
        _WindStrength("Wind Strength", Float) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Cull Off

            CGPROGRAM
            #pragma target 4.6

            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma geometry geo
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 normal : NORMAL;
                float4 pos : POSITION;
            };

            struct v2g
            {
                float3 normal : NORMAL;
                float4 pos : POSITION;
            };

            struct g2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _TessellationDistanceMin;
            float _TessellationDistanceMax;
            uint _TessellationFactorMin;
            uint _TessellationFactorMax;

            float _GrassHeight;
            float _GrassWidth;
            float _GrassOffset;

            sampler2D _WindDistortionMap;
            float4 _WindDistortionMap_ST;

            float2 _WindFrequency;
            float _WindStrength;

            v2g vert(appdata v)
            {
                v2g o;
                o.normal = v.normal;
                o.pos = v.pos;
                return o;
            }

            struct tf {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            tf patchconstant(InputPatch<v2g, 3> patch)
            {
                float3 p0 = length(UnityObjectToViewPos(patch[0].pos).xyz);
                float3 p1 = length(UnityObjectToViewPos(patch[1].pos).xyz);
                float3 p2 = length(UnityObjectToViewPos(patch[2].pos).xyz);

                tf f;
                f.edge[0] = _TessellationFactorMin + (_TessellationFactorMax - _TessellationFactorMin) *
                    (1 - (min(max(0, p0 - _TessellationDistanceMax), _TessellationDistanceMin) / (_TessellationDistanceMin - _TessellationDistanceMax)));
                f.edge[1] = _TessellationFactorMin + (_TessellationFactorMax - _TessellationFactorMin) * 
                    (1 - (min(max(0, p1 - _TessellationDistanceMax), _TessellationDistanceMin) / (_TessellationDistanceMin - _TessellationDistanceMax)));
                f.edge[2] = _TessellationFactorMin + (_TessellationFactorMax - _TessellationFactorMin) *
                    (1 - (min(max(0, p2 - _TessellationDistanceMax), _TessellationDistanceMin) / (_TessellationDistanceMin - _TessellationDistanceMax)));
                f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) * (1 / 3.0);
                return f;
            }

            [UNITY_domain("tri")]
            [UNITY_outputcontrolpoints(3)]
            [UNITY_outputtopology("triangle_cw")]
            [UNITY_partitioning("integer")]
            [UNITY_patchconstantfunc("patchconstant")]
            v2g hull(InputPatch<v2g, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            #define DOMAIN_INTERPOLATE(fieldName) data.fieldName = \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z;

            [UNITY_domain("tri")]
            v2g domain(tf factors, OutputPatch<v2g, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
            {
                v2g data;
                DOMAIN_INTERPOLATE(pos)
                DOMAIN_INTERPOLATE(normal)
                return data;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            g2f GenerateVertex(float3 pos, float3 offset, float2 uv)
            {
                matrix mv = UNITY_MATRIX_MV;
                mv[0][0] = 1;
                mv[1][0] = 0;
                mv[2][0] = 0;
                mv[0][1] = 0;
                mv[1][1] = 1;
                mv[2][1] = 0;
                mv[0][2] = 0;
                mv[1][2] = 0;
                mv[2][2] = 1;

                g2f o;
                o.uv = uv;
                o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(pos, 1.0)) + mul(mv, float4(offset, 0.0)));
                return o;
            }

            [maxvertexcount(4)]
            void geo(triangle v2g IN[3] : SV_POSITION, inout TriangleStream<g2f> triStream)
            {
                float4 pos = IN[0].pos;
                float3 normal = IN[0].normal;

                pos += float4(1, 0, 0, 0) * random(pos.xz) * _GrassOffset;
                pos += float4(0, 0, 1, 0) * random(pos.zx) * _GrassOffset;

                triStream.Append(GenerateVertex(pos, float3(-_GrassWidth, 0, 0), float2(0, 0)));
                triStream.Append(GenerateVertex(pos, float3( _GrassWidth, 0, 0), float2(1, 0)));

                float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
                float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
                float3 wind = float3(windSample.x, 0, windSample.y);

                triStream.Append(GenerateVertex(pos + wind + (_GrassHeight - length(wind)) *normal, float3(-_GrassWidth, 0, 0), float2(0, 1)));
                triStream.Append(GenerateVertex(pos + wind + (_GrassHeight - length(wind)) * normal, float3( _GrassWidth, 0, 0), float2(1, 1)));

                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target
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
