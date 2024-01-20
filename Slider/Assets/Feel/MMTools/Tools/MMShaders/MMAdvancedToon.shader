// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MoreMountains/MMAdvancedToon"
{
    Properties
    {
        _Cutoff( "Mask Clip Value", Float ) = 0.5
        [Header(Albedo)]_MainTex("MainTex", 2D) = "white" {}
        _Tint("Tint", Color) = (1,1,1,1)
        [Header(Normal Map)]_Normal("Normal", 2D) = "bump" {}
        [Header(Ramp Texture)][Toggle(_USERAMPTEXTURE_ON)] _UseRampTexture("UseRampTexture", Float) = 0
        _RampTexture("RampTexture", 2D) = "white" {}
        [Header(Generated Ramp)]_RampDark("RampDark", Color) = (0.3490566,0.3490566,0.3490566,0)
        _RampLight("RampLight", Color) = (1,1,1,0)
        _StepWidth("StepWidth", Range( 0.05 , 1)) = 0.25
        [IntRange]_StepAmount("StepAmount", Range( 0 , 16)) = 2
        _RampOffset("RampOffset", Range( 0 , 1)) = 0.5
        [Header(Vertex Colors)][Toggle(_USEVERTEXCOLORS_ON)] _UseVertexColors("UseVertexColors", Float) = 0
        [Header(Shadow)]_ShadowColor("ShadowColor", Color) = (1,0,0.115766,1)
        _LightColor("LightColor", Color) = (1,1,1,1)
        _ShadowBlur("ShadowBlur", Range( 0.01 , 1)) = 1
        _ShadowStrength("ShadowStrength", Range( 0 , 1)) = 1
        _ShadowSize("ShadowSize", Range( 0.01 , 1)) = 0.5
        [KeywordEnum(Multiply,Replace,Lighten,HardMix)] _ShadowMixMode("ShadowMixMode", Float) = 0
        [Header(Specular)][Toggle(_USESPECULAR_ON)] _UseSpecular("UseSpecular", Float) = 0
        _SpecularSize("SpecularSize", Range( 0 , 1)) = 0.4
        _SpecularFalloff("SpecularFalloff", Range( 0 , 2)) = 1
        [HDR]_SpecularColor("SpecularColor", Color) = (2,2,2,1)
        _SpecularPower("SpecularPower", Float) = 1
        _SpecularForceUnderShadow("SpecularForceUnderShadow", Float) = 0
        [Header(Rim Light)][Toggle(_USERIMLIGHT_ON)] _UseRimLight("UseRimLight", Float) = 0
        _RimColor("RimColor", Color) = (0,0.7342432,1,1)
        _RimPower("RimPower", Range( 0 , 1)) = 0.6547081
        _RimAmount("RimAmount", Range( 0 , 1)) = 0.7
        [Toggle(_HIDERIMUNDERSHADOW_ON)] _HideRimUnderShadow("HideRimUnderShadow", Float) = 0
        [Toggle(_SHARPRIMLIGHT_ON)] _SharpRimLight("SharpRimLight", Float) = 1
        [Header(Emission)]_EmissionTexture("EmissionTexture", 2D) = "white" {}
        [HDR]_EmissionColor("EmissionColor", Color) = (2,2,2,1)
        _EmissionForce("EmissionForce", Float) = 0
        [Header(Animation)]_Framerate("Framerate", Float) = 5
        [Header(VertexOffset)][Toggle(_USEVERTEXOFFSET_ON)] _UseVertexOffset("UseVertexOffset", Float) = 0
        _VertexOffsetNoiseTexture("VertexOffsetNoiseTexture", 2D) = "white" {}
        _VertexOffsetFrequency("VertexOffsetFrequency", Float) = 2
        _VertexOffsetMagnitude("VertexOffsetMagnitude", Float) = 0.05
        _VertexOffsetX("VertexOffsetX", Float) = 0.5
        _VertexOffsetY("VertexOffsetY", Float) = 0.5
        _VertexOffsetZ("VertexOffsetZ", Float) = 0.5
        [Header(Outline)]_OutlineColor("OutlineColor", Color) = (0.5451996,1,0,1)
        _OutlineWidth("OutlineWidth", Float) = 0.1
        _OutlineAlpha("OutlineAlpha", Range( 0 , 1)) = 0
        [Header(SecondaryTexture)]_SecondaryTexture("SecondaryTexture", 2D) = "white" {}
        _SecondaryTextureStrength("SecondaryTextureStrength", Float) = 0
        _SecondaryTextureSize("SecondaryTextureSize", Float) = 1
        _SecondaryTextureSpeedFactor("SecondaryTextureSpeedFactor", Float) = 0
        [Header(ToneMapping)]_Desaturation("Desaturation", Range( 0 , 1)) = 0
        _Contrast("Contrast", Range( -1 , 0.99)) = 0
        [HideInInspector] _texcoord( "", 2D ) = "white" {}
        [HideInInspector] __dirty( "", Int ) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout" "Queue" = "AlphaTest+0"
        }
        Cull Front
        CGPROGRAM
        #pragma target 3.0
        #pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc
        void outlineVertexDataFunc(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float outlineVar = _OutlineWidth;
            v.vertex.xyz += (v.normal * outlineVar);
        }

        inline half4 LightingOutline(SurfaceOutput s, half3 lightDir, half atten) { return half4(0, 0, 0, s.Alpha); }

        void outlineSurf(Input i, inout SurfaceOutput o)
        {
            o.Emission = _OutlineColor.rgb;
            clip(_OutlineAlpha - _Cutoff);
        }
        ENDCG


        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry+0" "IsEmissive" = "true"
        }
        Cull Back
        CGINCLUDE
        #include "UnityPBSLighting.cginc"
        #include "UnityShaderVariables.cginc"
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        #pragma shader_feature_local _USEVERTEXOFFSET_ON
        #pragma shader_feature_local _SHADOWMIXMODE_MULTIPLY _SHADOWMIXMODE_REPLACE _SHADOWMIXMODE_LIGHTEN _SHADOWMIXMODE_HARDMIX
        #pragma shader_feature_local _USESPECULAR_ON
        #pragma shader_feature_local _USERAMPTEXTURE_ON
        #pragma shader_feature_local _USEVERTEXCOLORS_ON
        #pragma shader_feature_local _USERIMLIGHT_ON
        #pragma shader_feature_local _SHARPRIMLIGHT_ON
        #pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
        #ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
        #endif
        struct Input
        {
            float2 uv_texcoord;
            float3 vertexToFrag80;
            float3 worldPos;
            float4 vertexColor : COLOR;
            float3 worldNormal;
            INTERNAL_DATA
        };

        struct SurfaceOutputCustomLightingCustom
        {
            half3 Albedo;
            half3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            half Alpha;
            Input SurfInput;
            UnityGIInput GIData;
        };

        uniform float _VertexOffsetMagnitude;
        uniform sampler2D _VertexOffsetNoiseTexture;
        uniform float _Framerate;
        uniform float _VertexOffsetFrequency;
        uniform float _VertexOffsetX;
        uniform float _VertexOffsetY;
        uniform float _VertexOffsetZ;
        uniform sampler2D _EmissionTexture;
        uniform float4 _EmissionTexture_ST;
        uniform float4 _EmissionColor;
        uniform float _EmissionForce;
        uniform float4 _RampDark;
        uniform float4 _RampLight;
        uniform sampler2D _Normal;
        uniform float4 _Normal_ST;
        uniform float _StepWidth;
        uniform float _StepAmount;
        uniform float _RampOffset;
        uniform sampler2D _RampTexture;
        uniform sampler2D _SecondaryTexture;
        uniform float _SecondaryTextureSize;
        uniform float _SecondaryTextureSpeedFactor;
        uniform float _SecondaryTextureStrength;
        uniform sampler2D _MainTex;
        uniform float4 _MainTex_ST;
        uniform float4 _Tint;
        uniform float _SpecularPower;
        uniform float _SpecularSize;
        uniform float _SpecularFalloff;
        uniform float4 _ShadowColor;
        uniform float _ShadowStrength;
        uniform float4 _LightColor;
        uniform float _ShadowSize;
        uniform float _ShadowBlur;
        uniform float _SpecularForceUnderShadow;
        uniform float4 _SpecularColor;
        uniform float _RimAmount;
        uniform float _RimPower;
        uniform float4 _RimColor;
        uniform float _Desaturation;
        uniform float _Contrast;
        uniform float _OutlineWidth;
        uniform float4 _OutlineColor;
        uniform float _OutlineAlpha;
        uniform float _Cutoff = 0.5;

        void vertexDataFunc(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float4 temp_cast_0 = (0.0).xxxx;
            half steppedTime293 = (round((_Time.y * _Framerate)) / _Framerate);
            float3 ase_vertex3Pos = v.vertex.xyz;
            float3 temp_output_281_0 = (ase_vertex3Pos * _VertexOffsetFrequency);
            half2 vertexOffsetXUV302 = (steppedTime293 + (temp_output_281_0).xy);
            half2 vertexOffsetYUV303 = ((steppedTime293 * 2.0) + (temp_output_281_0).yz);
            half2 vertexOffsetZUV304 = ((steppedTime293 * 4.0) + (temp_output_281_0).xz);
            float4 appendResult308 = (float4(
                (tex2Dlod(_VertexOffsetNoiseTexture, float4(vertexOffsetXUV302, 0, 0.0)).r - _VertexOffsetX),
                (tex2Dlod(_VertexOffsetNoiseTexture, float4(vertexOffsetYUV303, 0, 0.0)).r - _VertexOffsetY),
                (tex2Dlod(_VertexOffsetNoiseTexture, float4(vertexOffsetZUV304, 0, 0.0)).r - _VertexOffsetZ), 0.0));
            #ifdef _USEVERTEXOFFSET_ON
            float4 staticSwitch350 = (_VertexOffsetMagnitude * appendResult308);
            #else
				float4 staticSwitch350 = temp_cast_0;
            #endif
            float3 vertexOffset311 = (staticSwitch350).xyz;
            float3 outline364 = 0;
            v.vertex.xyz += (vertexOffset311 + outline364);
            float2 uv_Normal = v.texcoord * _Normal_ST.xy + _Normal_ST.zw;
            float3 normal83 = UnpackNormal(tex2Dlod(_Normal, float4(uv_Normal, 0, 0.0)));
            float3 ase_worldNormal = UnityObjectToWorldNormal(v.normal);
            float3 ase_worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
            float3x3 tangentToWorld = CreateTangentToWorldPerVertex(ase_worldNormal, ase_worldTangent, v.tangent.w);
            float3 tangentNormal33 = normal83;
            float3 modWorldNormal33 = normalize(
                (tangentToWorld[0] * tangentNormal33.x + tangentToWorld[1] * tangentNormal33.y + tangentToWorld[2] *
                    tangentNormal33.z));
            o.vertexToFrag80 = modWorldNormal33;
        }

        inline half4 LightingStandardCustomLighting(inout SurfaceOutputCustomLightingCustom s, half3 viewDir,
                                                    UnityGI gi)
        {
            UnityGIInput data = s.GIData;
            Input i = s.SurfInput;
            half4 c = 0;
            #ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
            #else
            float3 ase_lightAttenRGB = gi.light.color / ((_LightColor0.rgb) + 0.000001);
            float ase_lightAtten = max(max(ase_lightAttenRGB.r, ase_lightAttenRGB.g), ase_lightAttenRGB.b);
            #endif
            #if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
            #endif
            float3 normalizeResult81 = normalize(i.vertexToFrag80);
            float3 ase_worldPos = i.worldPos;
            #if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
            #else //aseld
            float3 ase_worldlightDir = Unity_SafeNormalize(UnityWorldSpaceLightDir(ase_worldPos));
            #endif //aseld
            float dotResult34 = dot(normalizeResult81, ase_worldlightDir);
            float NdotL31 = dotResult34;
            float4 lerpResult277 = lerp(_RampDark, _RampLight,
                                        saturate(((floor((NdotL31 / _StepWidth)) / _StepAmount) * 0.5 + _RampOffset)));
            float2 temp_cast_1 = (saturate((NdotL31 * 0.5 + 0.5))).xx;
            #ifdef _USERAMPTEXTURE_ON
            float4 staticSwitch3 = tex2D(_RampTexture, temp_cast_1);
            #else
				float4 staticSwitch3 = lerpResult277;
            #endif
            float4 ramp51 = staticSwitch3;
            #if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc


			float4 ase_lightColor = 0;
            #else //aselc
            float4 ase_lightColor = _LightColor0;
            #endif //aselc
            half steppedTime293 = (round((_Time.y * _Framerate)) / _Framerate);
            float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
            float4 temp_cast_3 = (1.0).xxxx;
            #ifdef _USEVERTEXCOLORS_ON
            float4 staticSwitch7 = i.vertexColor;
            #else
				float4 staticSwitch7 = temp_cast_3;
            #endif
            float4 blendOpSrc460 = (tex2D(_SecondaryTexture,
                                          ((i.uv_texcoord * _SecondaryTextureSize) + (steppedTime293 *
                                              _SecondaryTextureSpeedFactor))) * _SecondaryTextureStrength);
            float4 blendOpDest460 = ((tex2D(_MainTex, uv_MainTex) * _Tint) * staticSwitch7);
            float4 albedo11 = (saturate((blendOpDest460 - blendOpSrc460)));
            float4 temp_output_73_0 = ((ramp51 * float4(ase_lightColor.rgb, 0.0)) * albedo11);
            float temp_output_120_0 = (1.0 - _SpecularSize);
            float3 ase_worldViewDir = normalize(UnityWorldSpaceViewDir(ase_worldPos));
            float3 ase_worldNormal = WorldNormalVector(i, float3( 0, 0, 1 ));
            float dotResult106 = dot(ase_worldViewDir, ase_worldNormal);
            float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
            float3 normal83 = UnpackNormal(tex2D(_Normal, uv_Normal));
            float dotResult110 = dot(ase_worldViewDir, -reflect(ase_worldlightDir, (WorldNormalVector(i, normal83))));
            float specular113 = (pow(dotResult106, _SpecularFalloff) * dotResult110);
            float specularDelta116 = fwidth(specular113);
            float smoothstepResult121 = smoothstep(temp_output_120_0, (temp_output_120_0 + specularDelta116),
                                                   specular113);
            float temp_output_2_0_g2 = _ShadowStrength;
            float temp_output_3_0_g2 = (1.0 - temp_output_2_0_g2);
            float3 appendResult7_g2 = (float3(temp_output_3_0_g2, temp_output_3_0_g2, temp_output_3_0_g2));
            float clampResult189 = clamp(ase_lightAtten, 0.0, 1.0);
            float lerpResult409 = lerp(clampResult189, step(_ShadowSize, clampResult189), _ShadowBlur);
            float temp_output_191_0 = pow(lerpResult409, _ShadowBlur);
            float4 lerpResult194 = lerp(float4(((_ShadowColor.rgb * temp_output_2_0_g2) + appendResult7_g2), 0.0),
                                        _LightColor, temp_output_191_0);
            float4 shadow195 = lerpResult194;
            float4 temp_cast_7 = (_SpecularForceUnderShadow).xxxx;
            float4 temp_output_274_0 = round(pow(max(shadow195, float4(0.9528302, 0.9528302, 0.9528302, 0)),
                                                 temp_cast_7));
            float4 specularIntensity124 = ((_SpecularPower * smoothstepResult121) * temp_output_274_0);
            float4 temp_output_131_0 = (specular113 * _SpecularColor * saturate(specularIntensity124));
            float4 computedSpecular133 = temp_output_131_0;
            #ifdef _USESPECULAR_ON
            float4 staticSwitch137 = ((temp_output_73_0 * (1.0 - specularIntensity124)) + computedSpecular133);
            #else
				float4 staticSwitch137 = temp_output_73_0;
            #endif
            float4 litColor422 = staticSwitch137;
            float shadowArea411 = temp_output_191_0;
            float4 blendOpSrc410 = litColor422;
            float4 blendOpDest410 = shadow195;
            float4 blendOpSrc430 = litColor422;
            float4 blendOpDest430 = shadow195;
            #if defined(_SHADOWMIXMODE_MULTIPLY)
            float4 staticSwitch420 = (litColor422 * shadow195);
            #elif defined(_SHADOWMIXMODE_REPLACE)
				float4 staticSwitch420 = ( ( litColor422 * shadowArea411 ) + ( shadow195 * ( 1.0 - shadowArea411 ) ) );
            #elif defined(_SHADOWMIXMODE_LIGHTEN)
				float4 staticSwitch420 = ( saturate( 	max( blendOpSrc410, blendOpDest410 ) ));
            #elif defined(_SHADOWMIXMODE_HARDMIX)
				float4 staticSwitch420 = ( saturate(  round( 0.5 * ( blendOpSrc430 + blendOpDest430 ) ) ));
            #else
				float4 staticSwitch420 = ( litColor422 * shadow195 );
            #endif
            float4 shadowMix435 = staticSwitch420;
            float4 temp_cast_8 = (0.0).xxxx;
            float rimAmount169 = _RimAmount;
            float dotResult89 = dot((WorldNormalVector(i, normal83)), ase_worldViewDir);
            float NdotV90 = dotResult89;
            #ifdef _HIDERIMUNDERSHADOW_ON
            float staticSwitch166 = NdotL31;
            #else
				float staticSwitch166 = 1.0;
            #endif
            float temp_output_148_0 = ((1.0 - NdotV90) * pow(staticSwitch166, _RimPower));
            float smoothstepResult150 = smoothstep((rimAmount169 - 0.01), (0.01 + rimAmount169), temp_output_148_0);
            #ifdef _SHARPRIMLIGHT_ON
            float staticSwitch168 = smoothstepResult150;
            #else
				float staticSwitch168 = ( rimAmount169 * temp_output_148_0 );
            #endif
            #ifdef _USERIMLIGHT_ON
            float4 staticSwitch164 = (staticSwitch168 * _RimColor);
            #else
				float4 staticSwitch164 = temp_cast_8;
            #endif
            float4 rimLight157 = staticSwitch164;
            float4 preToneMapping438 = (shadowMix435 + rimLight157);
            float grayscale442 = Luminance(preToneMapping438.rgb);
            float4 temp_cast_10 = (grayscale442).xxxx;
            float4 lerpResult444 = lerp(preToneMapping438, temp_cast_10, _Desaturation);
            float4 temp_cast_11 = (_Contrast).xxxx;
            float4 postToneMapping439 = (float4(0, 0, 0, 0) + (lerpResult444 - temp_cast_11) * (float4(1, 1, 1, 0) -
                float4(0, 0, 0, 0)) / (float4(1, 1, 1, 0) - temp_cast_11));
            float4 lightCol68 = postToneMapping439;
            c.rgb = lightCol68.rgb;
            c.a = 1;
            return c;
        }

        inline void LightingStandardCustomLighting_GI(inout SurfaceOutputCustomLightingCustom s, UnityGIInput data,
                                                      inout UnityGI gi)
        {
            s.GIData = data;
        }

        void surf(Input i, inout SurfaceOutputCustomLightingCustom o)
        {
            o.SurfInput = i;
            o.Normal = float3(0, 0, 1);
            float2 uv_EmissionTexture = i.uv_texcoord * _EmissionTexture_ST.xy + _EmissionTexture_ST.zw;
            float4 computedEmission182 = ((tex2D(_EmissionTexture, uv_EmissionTexture) * _EmissionColor) *
                _EmissionForce);
            o.Emission = computedEmission182.rgb;
        }
        ENDCG
        CGPROGRAM
        #pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc
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
                float2 customPack1 : TEXCOORD1;
                float3 customPack2 : TEXCOORD2;
                float4 tSpace0 : TEXCOORD3;
                float4 tSpace1 : TEXCOORD4;
                float4 tSpace2 : TEXCOORD5;
                half4 color : COLOR0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_full v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                Input customInputData;
                vertexDataFunc(v, customInputData);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
                o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
                o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
                o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
                o.customPack1.xy = customInputData.uv_texcoord;
                o.customPack1.xy = v.texcoord;
                o.customPack2.xyz = customInputData.vertexToFrag80;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.color = v.color;
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
                surfIN.uv_texcoord = IN.customPack1.xy;
                surfIN.vertexToFrag80 = IN.customPack2.xyz;
                float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                surfIN.worldPos = worldPos;
                surfIN.worldNormal = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);
                surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
                surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
                surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
                surfIN.vertexColor = IN.color;
                SurfaceOutputCustomLightingCustom o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutputCustomLightingCustom, o)
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
    //CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16800
-1898;184;1873;1126;4029.691;4075.657;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;84;-6398.291,-2500.077;Float;False;810.3552;580.1461;Normal Map;2;82;83;Normal Map;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;82;-6342.859,-2240.926;Float;True;Property;_Normal;Normal;2;0;Create;True;0;0;True;1;Header(Normal Map);None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;135;-4270.331,273.337;Float;False;2352.279;2466.376;Specular;39;224;133;131;119;126;129;127;124;121;122;123;117;120;116;118;115;114;113;112;108;110;107;109;106;104;111;101;105;99;100;98;244;245;247;248;269;271;272;274;Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-5943.662,-2244.336;Float;False;normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-4220.331,1041.668;Float;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;35;-6394.275,-1621.117;Float;False;1336.096;443.0846;NdotL;7;85;31;34;81;32;80;33;NdotL;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;99;-3991.64,1032.872;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;100;-4018.027,864.2838;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;210;-4316.516,-1149.049;Float;False;3210.925;1296.744;Shadow;14;191;190;189;188;195;194;204;192;198;193;407;408;409;411;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.ReflectOpNode;101;-3720.432,980.0969;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;105;-4040.847,367.337;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;111;-4045.882,527.1082;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;85;-6355.366,-1550.021;Float;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;104;-3497.604,980.0958;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightAttenuation;188;-3532.483,-798.512;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-3805.461,615.0662;Float;False;Property;_SpecularFalloff;SpecularFalloff;19;0;Create;True;0;0;True;0;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;106;-3685.25,453.8093;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;109;-3541.583,745.5388;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;33;-6122.435,-1556.394;Float;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.VertexToFragmentNode;80;-5906.466,-1539.523;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;189;-3206.193,-689.9624;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;110;-3286.501,890.6708;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;408;-3023.774,-802.4628;Float;False;Property;_ShadowSize;ShadowSize;15;0;Create;True;0;0;True;0;0.5;0.281;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;108;-3496.139,481.6629;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;81;-5646.468,-1541.523;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;378;-6382.085,-400.4365;Float;False;1421.198;341.5015;Comment;6;288;289;290;291;292;293;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;32;-6102.436,-1366.394;Float;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;190;-2990.67,-102.864;Float;False;Property;_ShadowBlur;ShadowBlur;13;0;Create;True;0;0;True;0;1;0.538;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-3053.41,683.9678;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;407;-2811.39,-640.3524;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;288;-6332.085,-173.935;Float;False;Property;_Framerate;Framerate;32;0;Create;True;0;0;False;1;Header(Animation);5;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;289;-6312.839,-350.4365;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;34;-5502.437,-1442.394;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;193;-2490.299,-1029.671;Float;False;Property;_ShadowColor;ShadowColor;11;0;Create;True;0;0;False;1;Header(Shadow);1,0,0.115766,1;0.3867923,0.3867923,0.3867923,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;409;-2603.832,-376.1035;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-2813.017,615.7037;Float;True;specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-2513.879,-815.6242;Float;False;Property;_ShadowStrength;ShadowStrength;14;0;Create;True;0;0;True;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;204;-2095.522,-941.0401;Float;True;Lerp White To;-1;;2;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;290;-6021.639,-288.0374;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;192;-2257.108,-683.5875;Float;False;Property;_LightColor;LightColor;12;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;191;-2214.937,-386.631;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-5327.133,-1423.386;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;52;-4303.229,-2444.127;Float;False;2993.004;1198.177;Ramp;20;51;3;4;76;37;36;38;277;50;276;275;78;93;79;49;94;41;40;39;6;Ramp;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3063.968,898.2288;Float;False;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-4121.739,-2254.808;Float;False;Property;_StepWidth;StepWidth;7;0;Create;True;0;0;True;0;0.25;0.25;0.05;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;194;-1831.83,-694.3508;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;291;-5831.839,-272.4373;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FWidthOpNode;115;-2826.564,896.7169;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4003.614,-2355.187;Float;False;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;-1417.53,-680.6537;Float;False;shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;40;-3758.223,-2310.115;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-2643.596,884.6198;Float;False;specularDelta;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;292;-5675.839,-264.6375;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-4092.129,1294.172;Float;False;Property;_SpecularSize;SpecularSize;18;0;Create;True;0;0;True;0;0.4;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-3797.932,1519.586;Float;False;116;specularDelta;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;41;-3594.712,-2315.081;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-3705.001,-2058.321;Float;False;Property;_StepAmount;StepAmount;8;1;[IntRange];Create;True;0;0;False;0;2;4;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;120;-3772.225,1306.375;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-4149.815,1702.134;Float;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;15;-4365.522,-4292.567;Float;False;2898.835;1634.152;Albedo;19;11;14;7;10;12;2;13;1;379;380;382;383;384;381;387;388;389;457;460;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;293;-5281.887,-262.0375;Half;False;steppedTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-3565.063,1433.395;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;379;-4204.608,-3137.401;Float;False;Property;_SecondaryTextureSize;SecondaryTextureSize;45;0;Create;True;0;0;True;0;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;269;-3759.183,1735.286;Float;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0.9528302,0.9528302,0.9528302,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-3301.19,-1534.746;Float;False;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;272;-3669.064,2027.191;Float;False;Property;_SpecularForceUnderShadow;SpecularForceUnderShadow;22;0;Create;True;0;0;True;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-4200.861,-2858.142;Float;False;Property;_SecondaryTextureSpeedFactor;SecondaryTextureSpeedFactor;46;0;Create;True;0;0;True;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;382;-4218.378,-3285.435;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-3361.386,-1414.372;Float;False;Constant;_RampScaleAndOffset;RampScaleAndOffset;8;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-4235.591,-2985.924;Float;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-3545.247,-1836.601;Float;False;Property;_RampOffset;RampOffset;9;0;Create;True;0;0;True;0;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-3567.75,-1934.395;Float;False;Constant;_RampScale;RampScale;7;0;Create;True;0;0;True;0;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-3654.278,1211.111;Float;False;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;49;-3337.018,-2218.369;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;-3877.554,-3233.795;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;388;-3890.861,-2978.142;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;78;-3229.247,-1969.252;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;271;-3353.143,1784.866;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;21.47;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;121;-3355.371,1338.375;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;37;-3021.036,-1476.643;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;244;-3270.683,1178.104;Float;False;Property;_SpecularPower;SpecularPower;21;0;Create;True;0;0;True;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;76;-2732.488,-1511.635;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3013.739,1316.68;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;275;-2920.151,-2366.576;Float;False;Property;_RampDark;RampDark;5;0;Create;True;0;0;True;1;Header(Generated Ramp);0.3490566,0.3490566,0.3490566,0;0.3490564,0.3490564,0.3490564,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;50;-2872.888,-1959.341;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-4195.963,-4028.957;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;13;-4315.522,-3674.198;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-4328.115,-3815.262;Float;False;Constant;_NoVertexColor;NoVertexColor;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-4217.489,-4242.567;Float;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;True;1;Header(Albedo);None;079f5a8d00f74c84181be0a872b4c39d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;274;-3033.939,1801.531;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;276;-2889.599,-2180.924;Float;False;Property;_RampLight;RampLight;6;0;Create;True;0;0;True;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;389;-3684.861,-3054.142;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-2482.249,-1603.606;Float;True;Property;_RampTexture;RampTexture;4;0;Create;True;0;0;True;0;52e66a9243cdfed44b5e906f5910d35b;52e66a9243cdfed44b5e906f5910d35b;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-3858.242,-4062.196;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;277;-2329.182,-2006.194;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;381;-3506.364,-3460.195;Float;True;Property;_SecondaryTexture;SecondaryTexture;43;0;Create;True;0;0;False;1;Header(SecondaryTexture);None;6c6bb279e7b03f7448a324ac8aa5ac91;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;380;-3502.459,-3222.047;Float;False;Property;_SecondaryTextureStrength;SecondaryTextureStrength;44;0;Create;True;0;0;True;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;7;-4068.787,-3745.562;Float;False;Property;_UseVertexColors;UseVertexColors;10;0;Create;True;0;0;True;1;Header(Vertex Colors);0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;247;-2795.206,1632.378;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-2255.071,1262.254;Float;True;specularIntensity;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;3;-1964.97,-1941.765;Float;True;Property;_UseRampTexture;UseRampTexture;3;0;Create;True;0;0;True;1;Header(Ramp Texture);0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-3658.77,-3889.004;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;457;-3169.096,-3442.503;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;460;-3006.056,-3726.161;Float;True;Subtract;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;175;-630.5005,-3179.538;Float;False;4182.399;1269.731;Custom Lighting;18;70;72;139;65;69;134;141;140;73;159;438;158;437;137;138;422;68;440;Custom Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-4124.793,2452.915;Float;False;124;specularIntensity;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;91;-6389.196,-936.1287;Float;False;1133.981;413.4298;NdotV;5;86;87;88;89;90;NdotV;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-1587.945,-1918.789;Float;False;ramp;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-6339.196,-878.3267;Float;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;119;-3987.218,2244.336;Float;False;Property;_SpecularColor;SpecularColor;20;1;[HDR];Create;True;0;0;True;0;2,2,2,1;2,2,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;65;-533.4999,-3082.537;Float;False;51;ramp;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2653.021,-3745.462;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;129;-3829.799,2458.054;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;126;-4000.241,1986.725;Float;True;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;69;-364.67,-2884.373;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;72;-189.4763,-2663.172;Float;False;11;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-185.6951,-2438.733;Float;True;124;specularIntensity;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;87;-6061.468,-886.1287;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;88;-6061.468,-706.6989;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-153.0082,-2981.326;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-3619.327,2223.57;Float;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;89;-5771.26,-811.2357;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-2474.236,2214.922;Float;True;computedSpecular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;140;92.18002,-2438.526;Float;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;98.37092,-2805.102;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;173;-4246.42,4273.515;Float;False;3617.067;1146.109;Rim Light;23;157;164;165;156;168;142;172;150;155;171;148;152;147;170;151;146;163;166;144;169;149;167;143;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;342.0722,-2535.595;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-4002.27,4458.294;Float;False;Property;_RimAmount;RimAmount;26;0;Create;True;0;0;True;0;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-4093.783,4677.084;Float;True;Constant;_DontHideRimUnderShadow;DontHideRimUnderShadow;20;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-5498.219,-809.6757;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;320.2142,-2215.196;Float;True;133;computedSpecular;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-4076.015,4938.533;Float;True;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-3817.804,5113.262;Float;False;Property;_RimPower;RimPower;25;0;Create;True;0;0;True;0;0.6547081;0.6547081;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;651.1277,-2478.866;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;-3654.976,4457.298;Float;False;rimAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;166;-3768.122,4805.354;Float;True;Property;_HideRimUnderShadow;HideRimUnderShadow;27;0;Create;True;0;0;True;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-3382.022,4583.278;Float;False;90;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;-3190.997,5000.548;Float;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-2933.597,5005.695;Float;False;Constant;_RimAmountAdjuster;RimAmountAdjuster;17;0;Create;True;0;0;False;0;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;137;899.6168,-2648.938;Float;True;Property;_UseSpecular;UseSpecular;17;0;Create;True;0;0;True;1;Header(Specular);0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;-1839.482,-323.2064;Float;False;shadowArea;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;146;-3097.397,4572.993;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;436;-637.6216,-1760.471;Float;False;1728.671;1740.177;Shadow Mix;16;412;419;426;417;427;249;418;421;414;415;410;430;420;250;425;435;Shadow Mix;1,1,1,1;0;0
Node;AmplifyShaderEditor.PowerNode;147;-3410.963,4913;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-2787.997,4639.093;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;152;-2664.497,5065.494;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;-2586.136,4469.81;Float;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;422;1228.756,-2683.629;Float;False;litColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;155;-2657.997,4913.394;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;412;-587.6216,-1075.267;Float;True;411;shadowArea;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;150;-2405.541,4849.111;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;332;-4249.92,5639.075;Float;False;3131.241;1853.44;Vertex Offset;37;311;362;350;359;310;308;309;315;331;329;314;327;326;323;328;330;313;322;325;324;304;302;303;299;297;295;294;301;286;285;287;300;298;296;281;280;279;Vertex Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;426;-523.2374,-1408.464;Float;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;-2379.181,4559.983;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;417;-282.1374,-907.5627;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;419;-307.2184,-1147.121;Float;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;427;-163.255,-525.0162;Float;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;279;-3584.773,6111.28;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;418;-17.89666,-1020.36;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;421;-195.0661,-410.7025;Float;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;414;-318.0788,-1385.818;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;42.56085,-1574.666;Float;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;168;-2116.708,4754.35;Float;False;Property;_SharpRimLight;SharpRimLight;28;0;Create;True;0;0;True;0;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;142;-1992.58,5045.396;Float;False;Property;_RimColor;RimColor;24;0;Create;True;0;0;True;0;0,0.7342432,1,1;0,0.7342432,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;280;-3656.927,6318.275;Float;False;Property;_VertexOffsetFrequency;VertexOffsetFrequency;35;0;Create;True;0;0;True;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;425;51.66533,-1710.471;Float;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;415;221.7419,-1117.513;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;296;-2970.528,6241.574;Float;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;281;-3277.327,6185.674;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-1699.836,4854.964;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-1640.464,4699.232;Float;False;Constant;_DefaultRimLight;DefaultRimLight;19;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-2967.928,6470.375;Float;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;430;221.9494,-278.2941;Float;True;HardMix;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;367.6071,-1629.438;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;410;226.3874,-590.3668;Float;True;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;164;-1408.503,4788.544;Float;False;Property;_UseRimLight;UseRimLight;23;0;Create;True;0;0;True;1;Header(Rim Light);0;0;0;True;;Toggle;2;Key0;Key1;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-2666.328,6214.274;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;301;-2658.528,6424.874;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;420;677.1989,-1358.637;Float;False;Property;_ShadowMixMode;ShadowMixMode;16;0;Create;True;0;0;False;0;0;0;0;True;;KeywordEnum;4;Multiply;Replace;Lighten;HardMix;Create;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;286;-2941.927,6344.274;Float;False;False;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;287;-2934.128,6575.674;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;-3022.528,6016.673;Float;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;285;-2939.327,6110.274;Float;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;435;848.0494,-1509.184;Float;False;shadowMix;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;-2502.528,6509.375;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;157;-1113.162,4847.988;Float;False;rimLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;297;-2520.728,6298.774;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;295;-2648.128,6066.074;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;303;-2352.263,6301.432;Half;False;vertexOffsetYUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;437;1278.146,-2407.613;Float;False;435;shadowMix;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;302;-2421.174,6069.311;Half;False;vertexOffsetXUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-2326.875,6497.284;Half;False;vertexOffsetZUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;1274.657,-2266.357;Float;False;157;rimLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;322;-4199.92,6551.378;Float;True;Property;_VertexOffsetNoiseTexture;VertexOffsetNoiseTexture;34;0;Create;True;0;0;False;0;f49792b520b64f64c90e3945b118fe8f;f49792b520b64f64c90e3945b118fe8f;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;324;-4183.92,7066.577;Float;False;303;vertexOffsetYUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;313;-4194.428,6829.375;Float;False;302;vertexOffsetXUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;325;-4183.92,7306.577;Float;False;304;vertexOffsetZUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;159;1624.691,-2350.295;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;327;-3844.72,7229.777;Float;True;Property;_TextureSample2;Texture Sample 2;34;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;438;1920.378,-2351.102;Float;False;preToneMapping;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;323;-3831.92,6660.178;Float;True;Property;_TextureSample0;Texture Sample 0;34;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;314;-3463.585,6942.798;Float;False;Property;_VertexOffsetX;VertexOffsetX;37;0;Create;True;0;0;True;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;330;-3456.605,7329.949;Float;False;Property;_VertexOffsetZ;VertexOffsetZ;39;0;Create;True;0;0;True;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;456;-624.9999,117.4403;Float;False;1569.145;562.9836;Tone Mapping;7;441;444;442;443;452;448;439;Tone Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-3459.805,7144.349;Float;False;Property;_VertexOffsetY;VertexOffsetY;38;0;Create;True;0;0;True;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;326;-3847.92,6989.777;Float;True;Property;_TextureSample1;Texture Sample 1;34;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;331;-3228.035,7276.808;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;315;-3225.48,6894.424;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;441;-574.9998,167.4404;Float;False;438;preToneMapping;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;329;-3231.234,7091.208;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;309;-2899.191,6772.206;Float;False;Property;_VertexOffsetMagnitude;VertexOffsetMagnitude;36;0;Create;True;0;0;False;0;0.05;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;308;-2810.996,6918.278;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCGrayscale;442;-312.9865,295.9985;Float;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;443;-406.1073,400.7591;Float;False;Property;_Desaturation;Desaturation;47;0;Create;True;0;0;False;1;Header(ToneMapping);0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;376;-4256.833,7640.004;Float;False;1278.943;484.4639;Outline;5;355;364;356;354;377;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;-2543.653,6865.912;Float;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;448;-77.80162,544.4241;Float;False;Property;_Contrast;Contrast;48;0;Create;True;0;0;True;0;0;0;-1;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-2546.756,6765.939;Float;False;Constant;_NoVertexOffset;NoVertexOffset;41;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;444;-49.92125,251.7669;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;187;-4270.963,3064.128;Float;False;2278.231;843.0205;Emission;6;182;180;178;179;176;177;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;177;-4093.892,3523.297;Float;False;Property;_EmissionColor;EmissionColor;30;1;[HDR];Create;True;0;0;True;0;2,2,2,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;377;-4188.056,7912.742;Float;False;Property;_OutlineAlpha;OutlineAlpha;42;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;350;-2275.896,6843.649;Float;False;Property;_UseVertexOffset;UseVertexOffset;33;0;Create;True;0;0;True;1;Header(VertexOffset);0;0;1;True;;Toggle;2;Key0;Key1;Create;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;354;-4187.602,7731.861;Float;False;Property;_OutlineColor;OutlineColor;40;0;Create;True;0;0;True;1;Header(Outline);0.5451996,1,0,1;0.5451995,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;355;-4159.714,7997.744;Float;False;Property;_OutlineWidth;OutlineWidth;41;0;Create;True;0;0;True;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;452;312.1986,427.4243;Float;True;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;3;COLOR;0,0,0,0;False;4;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;176;-4114.888,3231.99;Float;True;Property;_EmissionTexture;EmissionTexture;29;0;Create;True;0;0;True;1;Header(Emission);None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;179;-3756.952,3615.031;Float;False;Property;_EmissionForce;EmissionForce;31;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;362;-1924.862,6853.405;Float;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OutlineNode;356;-3833.203,7847.26;Float;False;0;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-3650.371,3452.438;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;439;676.1446,423.965;Float;False;postToneMapping;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;174;2280.463,-1682.002;Float;False;1258.342;857.4734;Final;6;0;71;184;312;365;366;Final;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;311;-1647.014,6860.691;Float;False;vertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-3390.29,3522.278;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;364;-3253.303,7802.121;Float;False;outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;440;2314.613,-2368.219;Float;False;439;postToneMapping;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;182;-2829.427,3517.93;Float;False;computedEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;365;2569.255,-963.0233;Float;False;364;outline;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;2558.938,-1079.056;Float;False;311;vertexOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;2654.833,-2380.81;Float;False;lightCol;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;2751.913,-1200.242;Float;False;68;lightCol;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;-2792.295,1910.664;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;366;2805.255,-1027.023;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;184;2705.276,-1381.75;Float;False;182;computedEmission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3132.494,-1422.768;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;MoreMountains/MMAdvancedToon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;False;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;83;0;82;0
WireConnection;99;0;98;0
WireConnection;101;0;100;0
WireConnection;101;1;99;0
WireConnection;104;0;101;0
WireConnection;106;0;105;0
WireConnection;106;1;111;0
WireConnection;33;0;85;0
WireConnection;80;0;33;0
WireConnection;189;0;188;0
WireConnection;110;0;109;0
WireConnection;110;1;104;0
WireConnection;108;0;106;0
WireConnection;108;1;107;0
WireConnection;81;0;80;0
WireConnection;112;0;108;0
WireConnection;112;1;110;0
WireConnection;407;0;408;0
WireConnection;407;1;189;0
WireConnection;34;0;81;0
WireConnection;34;1;32;0
WireConnection;409;0;189;0
WireConnection;409;1;407;0
WireConnection;409;2;190;0
WireConnection;113;0;112;0
WireConnection;204;1;193;0
WireConnection;204;2;198;0
WireConnection;290;0;289;2
WireConnection;290;1;288;0
WireConnection;191;0;409;0
WireConnection;191;1;190;0
WireConnection;31;0;34;0
WireConnection;194;0;204;0
WireConnection;194;1;192;0
WireConnection;194;2;191;0
WireConnection;291;0;290;0
WireConnection;115;0;114;0
WireConnection;195;0;194;0
WireConnection;40;0;39;0
WireConnection;40;1;6;0
WireConnection;116;0;115;0
WireConnection;292;0;291;0
WireConnection;292;1;288;0
WireConnection;41;0;40;0
WireConnection;120;0;118;0
WireConnection;293;0;292;0
WireConnection;122;0;120;0
WireConnection;122;1;117;0
WireConnection;269;0;224;0
WireConnection;49;0;41;0
WireConnection;49;1;94;0
WireConnection;383;0;382;0
WireConnection;383;1;379;0
WireConnection;388;0;384;0
WireConnection;388;1;387;0
WireConnection;78;0;49;0
WireConnection;78;1;93;0
WireConnection;78;2;79;0
WireConnection;271;0;269;0
WireConnection;271;1;272;0
WireConnection;121;0;123;0
WireConnection;121;1;120;0
WireConnection;121;2;122;0
WireConnection;37;0;36;0
WireConnection;37;1;38;0
WireConnection;37;2;38;0
WireConnection;76;0;37;0
WireConnection;245;0;244;0
WireConnection;245;1;121;0
WireConnection;50;0;78;0
WireConnection;274;0;271;0
WireConnection;389;0;383;0
WireConnection;389;1;388;0
WireConnection;4;1;76;0
WireConnection;10;0;1;0
WireConnection;10;1;2;0
WireConnection;277;0;275;0
WireConnection;277;1;276;0
WireConnection;277;2;50;0
WireConnection;381;1;389;0
WireConnection;7;1;12;0
WireConnection;7;0;13;0
WireConnection;247;0;245;0
WireConnection;247;1;274;0
WireConnection;124;0;247;0
WireConnection;3;1;277;0
WireConnection;3;0;4;0
WireConnection;14;0;10;0
WireConnection;14;1;7;0
WireConnection;457;0;381;0
WireConnection;457;1;380;0
WireConnection;460;0;457;0
WireConnection;460;1;14;0
WireConnection;51;0;3;0
WireConnection;11;0;460;0
WireConnection;129;0;127;0
WireConnection;87;0;86;0
WireConnection;70;0;65;0
WireConnection;70;1;69;1
WireConnection;131;0;126;0
WireConnection;131;1;119;0
WireConnection;131;2;129;0
WireConnection;89;0;87;0
WireConnection;89;1;88;0
WireConnection;133;0;131;0
WireConnection;140;0;139;0
WireConnection;73;0;70;0
WireConnection;73;1;72;0
WireConnection;141;0;73;0
WireConnection;141;1;140;0
WireConnection;90;0;89;0
WireConnection;138;0;141;0
WireConnection;138;1;134;0
WireConnection;169;0;143;0
WireConnection;166;1;167;0
WireConnection;166;0;149;0
WireConnection;137;1;73;0
WireConnection;137;0;138;0
WireConnection;411;0;191;0
WireConnection;146;0;163;0
WireConnection;147;0;166;0
WireConnection;147;1;144;0
WireConnection;148;0;146;0
WireConnection;148;1;147;0
WireConnection;152;0;151;0
WireConnection;152;1;170;0
WireConnection;422;0;137;0
WireConnection;155;0;170;0
WireConnection;155;1;151;0
WireConnection;150;0;148;0
WireConnection;150;1;155;0
WireConnection;150;2;152;0
WireConnection;172;0;171;0
WireConnection;172;1;148;0
WireConnection;417;0;412;0
WireConnection;418;0;419;0
WireConnection;418;1;417;0
WireConnection;414;0;426;0
WireConnection;414;1;412;0
WireConnection;168;1;172;0
WireConnection;168;0;150;0
WireConnection;415;0;414;0
WireConnection;415;1;418;0
WireConnection;281;0;279;0
WireConnection;281;1;280;0
WireConnection;156;0;168;0
WireConnection;156;1;142;0
WireConnection;430;0;427;0
WireConnection;430;1;421;0
WireConnection;250;0;425;0
WireConnection;250;1;249;0
WireConnection;410;0;427;0
WireConnection;410;1;421;0
WireConnection;164;1;165;0
WireConnection;164;0;156;0
WireConnection;300;0;296;0
WireConnection;301;0;298;0
WireConnection;420;1;250;0
WireConnection;420;0;415;0
WireConnection;420;2;410;0
WireConnection;420;3;430;0
WireConnection;286;0;281;0
WireConnection;287;0;281;0
WireConnection;285;0;281;0
WireConnection;435;0;420;0
WireConnection;299;0;301;0
WireConnection;299;1;287;0
WireConnection;157;0;164;0
WireConnection;297;0;300;0
WireConnection;297;1;286;0
WireConnection;295;0;294;0
WireConnection;295;1;285;0
WireConnection;303;0;297;0
WireConnection;302;0;295;0
WireConnection;304;0;299;0
WireConnection;159;0;437;0
WireConnection;159;1;158;0
WireConnection;327;0;322;0
WireConnection;327;1;325;0
WireConnection;438;0;159;0
WireConnection;323;0;322;0
WireConnection;323;1;313;0
WireConnection;326;0;322;0
WireConnection;326;1;324;0
WireConnection;331;0;327;1
WireConnection;331;1;330;0
WireConnection;315;0;323;1
WireConnection;315;1;314;0
WireConnection;329;0;326;1
WireConnection;329;1;328;0
WireConnection;308;0;315;0
WireConnection;308;1;329;0
WireConnection;308;2;331;0
WireConnection;442;0;441;0
WireConnection;310;0;309;0
WireConnection;310;1;308;0
WireConnection;444;0;441;0
WireConnection;444;1;442;0
WireConnection;444;2;443;0
WireConnection;350;1;359;0
WireConnection;350;0;310;0
WireConnection;452;0;444;0
WireConnection;452;1;448;0
WireConnection;362;0;350;0
WireConnection;356;0;354;0
WireConnection;356;2;377;0
WireConnection;356;1;355;0
WireConnection;178;0;176;0
WireConnection;178;1;177;0
WireConnection;439;0;452;0
WireConnection;311;0;362;0
WireConnection;180;0;178;0
WireConnection;180;1;179;0
WireConnection;364;0;356;0
WireConnection;182;0;180;0
WireConnection;68;0;440;0
WireConnection;248;0;274;0
WireConnection;248;1;131;0
WireConnection;366;0;312;0
WireConnection;366;1;365;0
WireConnection;0;2;184;0
WireConnection;0;13;71;0
WireConnection;0;11;366;0
ASEEND*/
//CHKSM=7C426F26649D3ADDAABEEB2A5DBD5EE12CEAFE05