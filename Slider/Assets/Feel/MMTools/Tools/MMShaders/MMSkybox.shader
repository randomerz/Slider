Shader "MoreMountains/MMSkybox"
{
    Properties
    {
        _TopColor("Top Color", Color) = (1,1,1,0)
        _BottomColor("Bottom Color", Color) = (1,0.6,0,0)
        _Saturation("Saturation", Float) = 1
        _Intensity("Intensity", Float) = 1
        [Toggle(_SCREENSPACE)] _ScreenSpace("Screen Space", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        CGINCLUDE
        #pragma target 3.0
        ENDCG
        Blend Off
        Cull Back
        ColorMask RGBA
        ZWrite On
        ZTest LEqual
        Offset 0 , 0



        Pass
        {
            Name "Unlit"
            Tags
            {
                "LightMode"="ForwardBase"
            }
            CGPROGRAM
            #ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
            #endif
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma shader_feature_local _SCREENSPACE

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                #ifdef ASE_NEEDS_FRAG_WORLD_POSITION
					float3 worldPos : TEXCOORD0;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
                float4 ase_texcoord1 : TEXCOORD1;
                float4 ase_texcoord2 : TEXCOORD2;
            };

            uniform float4 _TopColor;
            uniform float4 _BottomColor;
            uniform float _Saturation;
            uniform float _Intensity;


            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
                float4 screenPos = ComputeScreenPos(ase_clipPos);
                o.ase_texcoord2 = screenPos;
                o.ase_texcoord1 = v.vertex;
                float3 vertexValue = float3(0, 0, 0);
                #if ASE_ABSOLUTE_VERTEX_POS
					vertexValue = v.vertex.xyz;
                #endif
                vertexValue = vertexValue;
                #if ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
                #else
                v.vertex.xyz += vertexValue;
                #endif
                o.vertex = UnityObjectToClipPos(v.vertex);

                #ifdef ASE_NEEDS_FRAG_WORLD_POSITION
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 finalColor;
                #ifdef ASE_NEEDS_FRAG_WORLD_POSITION
					float3 WorldPosition = i.worldPos;
                #endif
                float4 screenPos = i.ase_texcoord2;
                float4 ase_screenPosNorm = screenPos / screenPos.w;
                ase_screenPosNorm.z = (UNITY_NEAR_CLIP_VALUE >= 0)
                                          ? ase_screenPosNorm.z
                                          : ase_screenPosNorm.z * 0.5 + 0.5;
                #ifdef _SCREENSPACE
                float staticSwitch13 = ase_screenPosNorm.y;
                #else
					float staticSwitch13 = i.ase_texcoord1.xyz.y;
                #endif
                float4 lerpResult3 = lerp(_BottomColor, _TopColor,
                                          pow(saturate((staticSwitch13 * _Saturation)), _Intensity));
                finalColor = lerpResult3;
                return finalColor;
            }
            ENDCG
        }
    }
}