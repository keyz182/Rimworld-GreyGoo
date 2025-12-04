// Based on https://gist.github.com/josephbk117/a0e06d34aadb43777a1e35ccde508551
Shader "Unlit/LiquidMetalCracked"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", color) = (0.35294117647058826,0.30980392156862746,0.24313725490196078,1)
        _Frequency("Frequency", Range(0,100)) = 30.0
        _Lacunarity("Lacunarity", Range(0,100)) = 2.0
        _Gain("Gain", Range(0,1)) = 0.5
        _Jitter("Jitter", Range(0,1)) = 0.35
        _Ratio("Ratio", Range(0,1)) = 0.7
        _Octaves("Octaves", Int) = 1
        _BG("Background", color) = (0.2,0.1411764705882353,0.09803921568627451,1)
        _BGFrequency("Background Frequency", Range(0,100)) = 15
        _BGOctaves("Background Octaves", Int) = 4
    }
    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 200

        Pass
        {                            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag            
            // make fog work
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "GPUVoronoiNoise2D.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Frequency;
            float _Ratio;
            int _Octaves;
            float4 _BG;
            float _BGFrequency;
            int _BGOctaves;

            struct Input
            {
                float2 uv_MainTex;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                fixed4 bg = _BG;
                
                float bg_n = fBm_F0(i.uv, _BGOctaves, _Frequency, 1,1,1);

                bg = bg*bg_n;
                
                fixed4 mask = tex2D(_MainTex, i.uv);

                float n = fBm_F1_F0(i.uv, _Octaves, _BGFrequency,1,1,1);

                fixed4 o = bg*_Ratio + (1-_Ratio)*n;
                
                o.a = mask.a;

                return o;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}