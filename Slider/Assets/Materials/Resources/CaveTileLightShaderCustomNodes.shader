Shader "Custom/CaveTileShader"
{
	Properties{
			_MainTex("Texture", 2D) = "white" {}
			//[Toggle(IsLit)] _Lit("IsLit", Float) = 0
	}

	SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "UsesCaveLights" = "Yes"}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass{
			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _MainTex;
			float4 _MainTex_ST;

			//Arrays need to be fixed for shaders
			uniform float4 lightPos[3];
			uniform float4 lightDir[3];
			uniform float lightRadius[3];
			uniform float lightArcAngle[3];

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				bool lit = false;
				
				for (uint index = 0; index < 3; i++) {
					
					bool distGood = length(lightPos[index].xyz - i.position.xyz) < lightRadius[0];
					lit = distGood;

					if (lit) {
						break;
					}
				}
				
				if (!lit) {
					if (col.r > 0.75) {
						col = fixed4(0.6, 0.6, 0.6, col.a);
					}
					else if (col.r > 0.5) {
						col = fixed4(0.4, 0.4, 0.4, col.a);
					}
					else {
						col = fixed4(0.1, 0.1, 0.1, col.a);
					}
				}

				col *= i.color;
				return col;
			}

			ENDCG
		}
	}
}