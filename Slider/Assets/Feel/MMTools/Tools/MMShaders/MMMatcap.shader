Shader "MoreMountains/MMMatcap"
{
    Properties
    {
        _MatcapTexture("MatcapTexture", 2D) = "white" {}
        [HideInInspector] __dirty( "", Int ) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry+0" "IsEmissive" = "true"
        }
        Cull Back
        ZTest LEqual
        CGINCLUDE
        #include "UnityShaderVariables.cginc"
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        struct Input
        {
            half3 worldNormal;
        };

        uniform sampler2D _MatcapTexture;

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            half3 ase_worldNormal = i.worldNormal;
            o.Emission = tex2D(_MatcapTexture,
                               ((mul(half4(ase_worldNormal, 0.0), UNITY_MATRIX_V).xyz * 0.5) + 0.5).xy).rgb;
            o.Alpha = 1;
        }
        ENDCG
        CGPROGRAM
        #pragma surface surf Standard keepalpha fullforwardshadows
        ENDCG
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile UNITY_PASS_SHADOWCASTER
            #pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
            #include "HLSLSupport.cginc"
            #if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
            #define CAN_SKIP_VPOS
            #endif
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = worldNormal;
                o.worldPos = worldPos;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            half4 frag(v2f IN
                #if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
                #endif
            ) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Input surfIN;
                UNITY_INITIALIZE_OUTPUT(Input, surfIN);
                float3 worldPos = IN.worldPos;
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                surfIN.worldNormal = IN.worldNormal;
                SurfaceOutputStandard o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o)
                surf(surfIN, o);
                #if defined( CAN_SKIP_VPOS )
                float2 vpos = IN.pos;
                #endif
                SHADOW_CASTER_FRAGMENT(IN)
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}