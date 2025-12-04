Shader "Unlit/LiquidMetalSimplex"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        GrabPass{ "_GrabTexture" }

        UsePass "Transparent/Diffuse/FORWARD"
        Pass
        {
            ZTest On
            ZWrite On
            Blend One Zero
            Lighting Off
            Fog{ Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog


            #include "UnityCG.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _GrabTexture;
            float4 _MainTex_ST;
			float4 _Color;

            float signcos(float v)
            {
                return cos(v) * 0.5 + 0.5;
            }

            float height(float2 p)
            {
                float2 uv = p;
	            float res = 1.;
                for (int i = 0; i < 3; i++) {
                    res += cos(uv.y*12.345 - _Time.x*32. + cos(res*12.234)*.2 + cos(uv.x*32.2345 + cos(uv.y*17.234)) ) + cos(uv.x*12.345);
    	            uv = uv.yx;
                    uv.x += res*.1;
                }
                return res;
            }

            float2 normal(float2 p) {
                const float2 NE = float2(.1,0.);
                return normalize(float2( height(p+NE)-height(p-NE),
                                       height(p+NE.yx)-height(p-NE.yx) ));
            }
            float3 diffuse(float2 p) {

                float2 uv = p;
	            float res = 1.;
                for (int i = 0; i < 3; i++) {
                    res += cos(uv.y*12.345 - _Time.x*32. + cos(res*12.234)*.2 + cos(uv.x*32.2345 + cos(uv.y*17.234)) ) + cos(uv.x*12.345);
    	            uv = uv.yx;
                    uv.x += res*.1;
                }

                return tex2D(_MainTex, uv).xyz;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.uv2 = ComputeGrabScreenPos(UnityObjectToClipPos(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 bg = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uv2));

                fixed4 tex = tex2D(_MainTex, UNITY_PROJ_COORD(i.uv));
                float2 uv = (i.vertex / _ScreenParams.xy - .05)/2;

                float3 lightDir = normalize(float3(sin(_Time.x),1.,cos(_Time.x)));

                float3 norm3d = normalize(float3(normal(uv),1.).xzy);
                float3 dif = diffuse(uv);
                dif *= .25+max(0.,dot(norm3d,lightDir));

                float3 view = normalize(float3(uv,-1.).xzy);
                float3 spec = tex2D(_MainTex, reflect(view, norm3d)).xyz* max(0.,dot(-norm3d,view));
                fixed4 genned = float4(lerp(dif,spec,.5), tex.a) ;

                fixed4 output = ((1- tex.a)*bg) + (tex.a * genned);

                float2 noiseUv = uv * 200;// + float2(0.2, 1) * _Time.y;
                float3 noiseInput;
                noiseInput.xy = noiseUv;
                noiseInput.z = _Time.x*60;

                float noise = SimplexNoise(noiseInput);

                output.xyz = output.xyz * ((noise + 4) / 6);

                return output;


            }
            ENDCG
        }
    }
}
