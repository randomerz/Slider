// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MoreMountains/MMVFX"
{
    Properties
    {
        _Cutoff( "Mask Clip Value", Float ) = 0.5
        [Header(Albedo)]_MainTex("MainTex", 2D) = "white" {}
        _MainTexPanningSpeed("MainTexPanningSpeed", Vector) = (0,0,0,0)
        _Tint("Tint", Color) = (1,1,1,1)
        [Header(Opacity)]_Opacity("Opacity", Range( 0 , 1)) = 1
        _OpacityMask("OpacityMask", Range( 0 , 1)) = 0.7814395
        [Header(Normal Map)]_Normal("Normal", 2D) = "bump" {}
        [Header(Vertex Colors)][Toggle(_USEVERTEXCOLORS_ON)] _UseVertexColors("UseVertexColors", Float) = 0
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
            "RenderType" = "Transparent" "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"
        }
        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        BlendOp Add
        CGINCLUDE
        #include "UnityShaderVariables.cginc"
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        #pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
        #pragma shader_feature_local _SHARPRIMLIGHT_ON
        #pragma shader_feature_local _USERIMLIGHT_ON
        #pragma shader_feature_local _USEVERTEXOFFSET_ON
        #pragma shader_feature_local _USEVERTEXCOLORS_ON
        struct Input
        {
            float2 uv_texcoord;
            float4 vertexColor : COLOR;
        };

        uniform sampler2D _Normal;
        uniform float _RimAmount;
        uniform float _RimPower;
        uniform float4 _RimColor;
        uniform float _VertexOffsetMagnitude;
        uniform sampler2D _VertexOffsetNoiseTexture;
        uniform float _Framerate;
        uniform float _VertexOffsetFrequency;
        uniform float _VertexOffsetX;
        uniform float _VertexOffsetY;
        uniform float _VertexOffsetZ;
        uniform sampler2D _SecondaryTexture;
        uniform float _SecondaryTextureSize;
        uniform float _SecondaryTextureSpeedFactor;
        uniform float _SecondaryTextureStrength;
        uniform sampler2D _MainTex;
        uniform float2 _MainTexPanningSpeed;
        uniform float4 _Tint;
        uniform sampler2D _EmissionTexture;
        uniform float4 _EmissionTexture_ST;
        uniform float4 _EmissionColor;
        uniform float _EmissionForce;
        uniform float _Opacity;
        uniform float _OpacityMask;
        uniform float _Cutoff = 0.5;
        uniform float4 _OutlineColor;
        uniform float _OutlineAlpha;
        uniform float _OutlineWidth;

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
            v.vertex.w = 1;
        }

        inline half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(0, 0, 0, s.Alpha);
        }

        void surf(Input i, inout SurfaceOutput o)
        {
            half steppedTime293 = (round((_Time.y * _Framerate)) / _Framerate);
            float2 panner462 = (1.0 * _Time.y * _MainTexPanningSpeed + i.uv_texcoord);
            float4 temp_cast_0 = (1.0).xxxx;
            #ifdef _USEVERTEXCOLORS_ON
            float4 staticSwitch7 = i.vertexColor;
            #else
				float4 staticSwitch7 = temp_cast_0;
            #endif
            float4 blendOpSrc460 = (tex2D(_SecondaryTexture,
                                          ((i.uv_texcoord * _SecondaryTextureSize) + (steppedTime293 *
                                              _SecondaryTextureSpeedFactor))) * _SecondaryTextureStrength);
            float4 blendOpDest460 = ((tex2D(_MainTex, panner462) * _Tint) * staticSwitch7);
            float4 albedo11 = (saturate((blendOpDest460 - blendOpSrc460)));
            float2 uv_EmissionTexture = i.uv_texcoord * _EmissionTexture_ST.xy + _EmissionTexture_ST.zw;
            float4 computedEmission182 = ((tex2D(_EmissionTexture, uv_EmissionTexture) * _EmissionColor) *
                _EmissionForce);
            o.Emission = (albedo11 * computedEmission182).rgb;
            float Opacity473 = _Opacity;
            o.Alpha = Opacity473;
            float4 temp_cast_2 = (_OpacityMask).xxxx;
            float4 OpacityMask468 = step(albedo11, temp_cast_2);
            clip(OpacityMask468.r - _Cutoff);
        }
        ENDCG
        CGPROGRAM
        #pragma surface surf Unlit keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc
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
            sampler3D _DitherMaskLOD;

            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 customPack1 : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half4 color : COLOR0;
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
                Input customInputData;
                vertexDataFunc(v, customInputData);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.customPack1.xy = customInputData.uv_texcoord;
                o.customPack1.xy = v.texcoord;
                o.worldPos = worldPos;
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
                float3 worldPos = IN.worldPos;
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                surfIN.vertexColor = IN.color;
                SurfaceOutput o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutput, o)
                surf(surfIN, o);
                #if defined( CAN_SKIP_VPOS )
                float2 vpos = IN.pos;
                #endif
                half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy * 0.25, o.Alpha * 0.9375)).a;
                clip(alphaRef - 0.01);
                SHADOW_CASTER_FRAGMENT(IN)
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
    //CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
2637;262;1828;996;1581.965;5374.667;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;378;-6382.085,-400.4365;Inherit;False;1421.198;341.5015;Comment;6;288;289;290;291;292;293;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TimeNode;289;-6312.839,-350.4365;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;288;-6332.085,-173.935;Float;False;Property;_Framerate;Framerate;17;0;Create;True;0;0;0;False;1;Header(Animation);False;5;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;290;-6021.639,-288.0374;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;291;-5831.839,-272.4373;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;332;-4249.92,5639.075;Inherit;False;3131.241;1853.44;Vertex Offset;37;311;362;350;359;310;308;309;315;331;329;314;327;326;323;328;330;313;322;325;324;304;302;303;299;297;295;294;301;286;285;287;300;298;296;281;280;279;Vertex Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;292;-5675.839,-264.6375;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;279;-3584.773,6111.28;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;293;-5281.887,-262.0375;Half;False;steppedTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;280;-3656.927,6318.275;Float;False;Property;_VertexOffsetFrequency;VertexOffsetFrequency;20;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-2967.928,6470.375;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;281;-3277.327,6185.674;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;296;-2970.528,6241.574;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;285;-2939.327,6110.274;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;301;-2658.528,6424.874;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-2666.328,6214.274;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;287;-2934.128,6575.674;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;286;-2941.927,6344.274;Inherit;False;False;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;-3022.528,6016.673;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;15;-5059.295,-4359.276;Inherit;False;2898.835;1634.152;Albedo;22;11;14;7;10;12;2;13;1;379;380;382;383;384;381;387;388;389;457;460;462;463;464;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;295;-2648.128,6066.074;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;-2502.528,6509.375;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;297;-2520.728,6298.774;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;382;-4615.27,-3342.866;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;464;-4839.599,-4217.588;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;379;-4601.5,-3194.832;Float;False;Property;_SecondaryTextureSize;SecondaryTextureSize;30;0;Create;True;0;0;0;True;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-4597.753,-2915.573;Float;False;Property;_SecondaryTextureSpeedFactor;SecondaryTextureSpeedFactor;31;0;Create;True;0;0;0;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-2326.875,6497.284;Half;False;vertexOffsetZUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-4632.483,-3043.355;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;303;-2352.263,6301.432;Half;False;vertexOffsetYUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;302;-2421.174,6069.311;Half;False;vertexOffsetXUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;463;-4832.088,-4012.864;Inherit;False;Property;_MainTexPanningSpeed;MainTexPanningSpeed;2;0;Create;True;0;0;0;False;0;False;0,0;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;462;-4551.144,-4127.432;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;-4274.446,-3291.226;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;388;-4287.753,-3035.573;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;313;-4194.428,6829.375;Inherit;False;302;vertexOffsetXUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;325;-4183.92,7306.577;Inherit;False;304;vertexOffsetZUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;322;-4199.92,6551.378;Float;True;Property;_VertexOffsetNoiseTexture;VertexOffsetNoiseTexture;19;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;324;-4183.92,7066.577;Inherit;False;303;vertexOffsetYUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;2;-4255.289,-3945.874;Float;False;Property;_Tint;Tint;3;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-4370.208,-4183.944;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;0;True;1;Header(Albedo);False;-1;5551f2320b5bfde4dbdbb0604036a53a;5551f2320b5bfde4dbdbb0604036a53a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;389;-4081.753,-3111.573;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;13;-4325.929,-3640.034;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-4322.957,-3749.968;Float;False;Constant;_NoVertexColor;NoVertexColor;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;314;-3463.585,6942.798;Float;False;Property;_VertexOffsetX;VertexOffsetX;22;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-3459.805,7144.349;Float;False;Property;_VertexOffsetY;VertexOffsetY;23;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;327;-3844.72,7229.777;Inherit;True;Property;_TextureSample2;Texture Sample 2;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;326;-3847.92,6989.777;Inherit;True;Property;_TextureSample1;Texture Sample 1;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;323;-3831.92,6660.178;Inherit;True;Property;_TextureSample0;Texture Sample 0;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;330;-3456.605,7329.949;Float;False;Property;_VertexOffsetZ;VertexOffsetZ;24;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;315;-3225.48,6894.424;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;329;-3231.234,7091.208;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;381;-3903.255,-3517.626;Inherit;True;Property;_SecondaryTexture;SecondaryTexture;28;0;Create;True;0;0;0;False;1;Header(SecondaryTexture);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;380;-3899.35,-3279.479;Float;False;Property;_SecondaryTextureStrength;SecondaryTextureStrength;29;0;Create;True;0;0;0;True;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;7;-4096.983,-3704.728;Float;False;Property;_UseVertexColors;UseVertexColors;7;0;Create;True;0;0;0;True;1;Header(Vertex Colors);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-3999.843,-4005.796;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;331;-3228.035,7276.808;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-3731.438,-3814.815;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;457;-3565.987,-3499.934;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;308;-2810.996,6918.278;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;309;-2899.191,6772.206;Float;False;Property;_VertexOffsetMagnitude;VertexOffsetMagnitude;21;0;Create;True;0;0;0;False;0;False;0.05;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;187;-1910.011,-4518.599;Inherit;False;2278.231;843.0205;Emission;6;182;180;178;179;176;177;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;176;-1753.937,-4350.736;Inherit;True;Property;_EmissionTexture;EmissionTexture;14;0;Create;True;0;0;0;True;1;Header(Emission);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;-2543.653,6865.912;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;177;-1732.941,-4059.429;Float;False;Property;_EmissionColor;EmissionColor;15;1;[HDR];Create;True;0;0;0;True;0;False;2,2,2,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;460;-3402.947,-3783.592;Inherit;True;Subtract;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;376;-4256.833,7640.004;Inherit;False;1278.943;484.4639;Outline;5;355;364;356;354;377;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-2546.756,6765.939;Float;False;Constant;_NoVertexOffset;NoVertexOffset;41;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-1289.42,-4130.288;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;179;-1396.001,-3967.695;Float;False;Property;_EmissionForce;EmissionForce;16;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2736.712,-3809.693;Float;True;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;475;-3035.437,-2407.879;Inherit;False;1193.9;692.5161;Opacity;6;467;465;472;468;473;479;Opacity;0.5599858,0.8641157,0.9811321,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;350;-2275.896,6843.649;Float;False;Property;_UseVertexOffset;UseVertexOffset;18;0;Create;True;0;0;0;True;1;Header(VertexOffset);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;377;-4188.056,7912.742;Float;False;Property;_OutlineAlpha;OutlineAlpha;27;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;355;-4159.714,7997.744;Float;False;Property;_OutlineWidth;OutlineWidth;26;0;Create;True;0;0;0;True;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;354;-4187.602,7731.861;Float;False;Property;_OutlineColor;OutlineColor;25;0;Create;True;0;0;0;True;1;Header(Outline);False;0.5451996,1,0,1;0.5451995,1,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-1029.339,-4060.448;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OutlineNode;356;-3833.203,7847.26;Inherit;False;0;True;Masked;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;362;-1924.862,6853.405;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;467;-2985.437,-1841.816;Inherit;False;Property;_OpacityMask;OpacityMask;5;0;Create;True;0;0;0;False;0;False;0.7814395;0.51;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;465;-2751.765,-2151.973;Inherit;True;11;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;174;-248.6374,-2625.146;Inherit;False;1258.342;857.4734;Final;9;0;184;312;365;366;469;474;476;477;Final;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;311;-1647.014,6860.691;Float;False;vertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;472;-2886.966,-2338.879;Inherit;False;Property;_Opacity;Opacity;4;0;Create;True;0;0;0;False;1;Header(Opacity);False;1;100;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;479;-2424.015,-2037.747;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;364;-3253.303,7802.121;Float;False;outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;182;-851.4753,-4008.796;Float;False;computedEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;84;-6398.291,-2500.077;Inherit;False;810.3552;580.1461;Normal Map;2;82;83;Normal Map;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;29.83773,-2022.2;Inherit;False;311;vertexOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;35;-6394.275,-1621.117;Inherit;False;1336.096;443.0846;NdotL;7;85;31;34;81;32;80;33;NdotL;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;473;-2543.967,-2345.879;Inherit;False;Opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;91;-6389.196,-936.1287;Inherit;False;1133.981;413.4298;NdotV;5;86;87;88;89;90;NdotV;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;184;-166.8244,-2331.894;Inherit;False;182;computedEmission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;365;40.15461,-1906.167;Inherit;False;364;outline;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;476;-60.46403,-2533.214;Inherit;True;11;albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;173;-4246.42,4273.515;Inherit;False;3617.067;1146.109;Rim Light;23;157;164;165;156;168;142;172;150;155;171;148;152;147;170;151;146;163;166;144;169;149;167;143;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;468;-2040.537,-2047.315;Inherit;False;OpacityMask;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;34;-5502.437,-1442.394;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;155;-2657.997,4913.394;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;477;262.4405,-2491.379;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-4076.015,4938.533;Inherit;True;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-1640.464,4699.232;Float;False;Constant;_DefaultRimLight;DefaultRimLight;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;206.7545,-2177.134;Inherit;False;468;OpacityMask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexToFragmentNode;80;-5906.466,-1539.523;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-5943.662,-2244.336;Float;False;normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;366;276.1544,-1970.167;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FresnelNode;485;-869.1298,-5093.487;Inherit;True;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;-3190.997,5000.548;Inherit;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-2933.597,5005.695;Float;False;Constant;_RimAmountAdjuster;RimAmountAdjuster;17;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-4093.783,4677.084;Float;True;Constant;_DontHideRimUnderShadow;DontHideRimUnderShadow;20;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;-2379.181,4559.983;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-6339.196,-878.3267;Inherit;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;489;-1204.588,-5110.409;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;488;-1232.588,-4964.407;Inherit;False;Constant;_Float0;Float 0;32;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;505;-489.8678,-5194.231;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;507;-331.4651,-4959.637;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;150;-2405.541,4849.111;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;164;-1408.503,4788.544;Float;False;Property;_UseRimLight;UseRimLight;8;0;Create;True;0;0;0;True;1;Header(Rim Light);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;82;-6342.859,-2240.926;Inherit;True;Property;_Normal;Normal;6;0;Create;True;0;0;0;True;1;Header(Normal Map);False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;163;-3382.022,4583.278;Inherit;False;90;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;157;-1113.162,4847.988;Float;False;rimLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;142;-1992.58,5045.396;Float;False;Property;_RimColor;RimColor;9;0;Create;True;0;0;0;True;0;False;0,0.7342432,1,1;0,0.7342432,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;81;-5646.468,-1541.523;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-2787.997,4639.093;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-4002.27,4458.294;Float;False;Property;_RimAmount;RimAmount;11;0;Create;True;0;0;0;True;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-3817.804,5113.262;Float;False;Property;_RimPower;RimPower;10;0;Create;True;0;0;0;True;0;False;0.6547081;0.6547081;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;33;-6122.435,-1556.394;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;474;208.0318,-2254.197;Inherit;False;473;Opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-5498.219,-809.6757;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;87;-6061.468,-886.1287;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;171;-2586.136,4469.81;Inherit;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;89;-5771.26,-811.2357;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-5327.133,-1423.386;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-1699.836,4854.964;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;166;-3768.122,4805.354;Float;True;Property;_HideRimUnderShadow;HideRimUnderShadow;12;0;Create;True;0;0;0;True;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;32;-6102.436,-1366.394;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;-3654.976,4457.298;Float;False;rimAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;146;-3097.397,4572.993;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;88;-6061.468,-706.6989;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PowerNode;147;-3410.963,4913;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;168;-2116.708,4754.35;Float;False;Property;_SharpRimLight;SharpRimLight;13;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;152;-2664.497,5065.494;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;-6355.366,-1550.021;Inherit;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;508;-519.9648,-4857.667;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;603.3936,-2365.912;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;MoreMountains/MMVFX;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;3;Custom;0.5;True;True;0;True;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;1;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;False;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;290;0;289;2
WireConnection;290;1;288;0
WireConnection;291;0;290;0
WireConnection;292;0;291;0
WireConnection;292;1;288;0
WireConnection;293;0;292;0
WireConnection;281;0;279;0
WireConnection;281;1;280;0
WireConnection;285;0;281;0
WireConnection;301;0;298;0
WireConnection;300;0;296;0
WireConnection;287;0;281;0
WireConnection;286;0;281;0
WireConnection;295;0;294;0
WireConnection;295;1;285;0
WireConnection;299;0;301;0
WireConnection;299;1;287;0
WireConnection;297;0;300;0
WireConnection;297;1;286;0
WireConnection;304;0;299;0
WireConnection;303;0;297;0
WireConnection;302;0;295;0
WireConnection;462;0;464;0
WireConnection;462;2;463;0
WireConnection;383;0;382;0
WireConnection;383;1;379;0
WireConnection;388;0;384;0
WireConnection;388;1;387;0
WireConnection;1;1;462;0
WireConnection;389;0;383;0
WireConnection;389;1;388;0
WireConnection;327;0;322;0
WireConnection;327;1;325;0
WireConnection;326;0;322;0
WireConnection;326;1;324;0
WireConnection;323;0;322;0
WireConnection;323;1;313;0
WireConnection;315;0;323;1
WireConnection;315;1;314;0
WireConnection;329;0;326;1
WireConnection;329;1;328;0
WireConnection;381;1;389;0
WireConnection;7;1;12;0
WireConnection;7;0;13;0
WireConnection;10;0;1;0
WireConnection;10;1;2;0
WireConnection;331;0;327;1
WireConnection;331;1;330;0
WireConnection;14;0;10;0
WireConnection;14;1;7;0
WireConnection;457;0;381;0
WireConnection;457;1;380;0
WireConnection;308;0;315;0
WireConnection;308;1;329;0
WireConnection;308;2;331;0
WireConnection;310;0;309;0
WireConnection;310;1;308;0
WireConnection;460;0;457;0
WireConnection;460;1;14;0
WireConnection;178;0;176;0
WireConnection;178;1;177;0
WireConnection;11;0;460;0
WireConnection;350;1;359;0
WireConnection;350;0;310;0
WireConnection;180;0;178;0
WireConnection;180;1;179;0
WireConnection;356;0;354;0
WireConnection;356;2;377;0
WireConnection;356;1;355;0
WireConnection;362;0;350;0
WireConnection;311;0;362;0
WireConnection;479;0;465;0
WireConnection;479;1;467;0
WireConnection;364;0;356;0
WireConnection;182;0;180;0
WireConnection;473;0;472;0
WireConnection;468;0;479;0
WireConnection;34;0;81;0
WireConnection;34;1;32;0
WireConnection;155;0;170;0
WireConnection;155;1;151;0
WireConnection;477;0;476;0
WireConnection;477;1;184;0
WireConnection;80;0;33;0
WireConnection;83;0;82;0
WireConnection;366;0;312;0
WireConnection;366;1;365;0
WireConnection;485;0;489;0
WireConnection;485;2;488;0
WireConnection;172;0;171;0
WireConnection;172;1;148;0
WireConnection;505;0;485;0
WireConnection;507;0;485;0
WireConnection;150;0;148;0
WireConnection;150;1;155;0
WireConnection;150;2;152;0
WireConnection;164;1;165;0
WireConnection;164;0;156;0
WireConnection;157;0;164;0
WireConnection;81;0;80;0
WireConnection;148;0;146;0
WireConnection;148;1;147;0
WireConnection;33;0;85;0
WireConnection;90;0;89;0
WireConnection;87;0;86;0
WireConnection;89;0;87;0
WireConnection;89;1;88;0
WireConnection;31;0;34;0
WireConnection;156;0;168;0
WireConnection;156;1;142;0
WireConnection;166;1;167;0
WireConnection;166;0;149;0
WireConnection;169;0;143;0
WireConnection;146;0;163;0
WireConnection;147;0;166;0
WireConnection;147;1;144;0
WireConnection;168;1;172;0
WireConnection;168;0;150;0
WireConnection;152;0;151;0
WireConnection;152;1;170;0
WireConnection;508;0;485;0
WireConnection;0;2;477;0
WireConnection;0;9;474;0
WireConnection;0;10;469;0
WireConnection;0;11;366;0
ASEEND*/
//CHKSM=A8A8600C8A2CBE75B3D76A19CCBA8959A6D98058