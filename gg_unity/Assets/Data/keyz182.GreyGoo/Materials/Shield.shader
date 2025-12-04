Shader "Unlit/Shield"
{
    Properties
    {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
		_FresnelBias ("Fresnel Bias", Float) = 0
		_FresnelScale ("Fresnel Scale", Float) = 1
		_FresnelPower ("Fresnel Power", Float) = 1
    }
    SubShader
    {
		Tags
		{
			"Queue"="Transparent"
//			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
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

            #include "UnityCG.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

			struct appdata_t
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : NORMAL;
			};


			struct v2f
			{
				float4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
                float4 uv2 : TEXCOORD1;
				float fresnel : TEXCOORD2;
			};

			sampler2D _MainTex;
            sampler2D _GrabTexture;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _FresnelColor;
			fixed _FresnelBias;
			fixed _FresnelScale;
			fixed _FresnelPower;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = ComputeGrabScreenPos(UnityObjectToClipPos(v.pos));

				float3 i = normalize(ObjSpaceViewDir(v.pos));
				o.fresnel = _FresnelBias + _FresnelScale * pow(1 + dot(i, v.normal), _FresnelPower);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
                fixed4 bg = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uv2));
				
				fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                fixed4 fres = lerp(c, _FresnelColor, 1 - i.fresnel);
				
                float2 noiseUv = i.uv * 2;// + float2(0.2, 1) * _Time.y;
                float3 noiseInput;
                noiseInput.xy = noiseUv;
                noiseInput.z = _Time.x*8;
				
                float noise = SimplexNoise(noiseInput);
				return bg * (1-fres) + 1 -((noise + 14) / 15);
			}
            ENDCG
        }
    }
}
