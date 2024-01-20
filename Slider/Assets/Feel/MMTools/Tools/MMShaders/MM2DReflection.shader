// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MoreMountains/MM2DReflection"
{
    Properties
    {
        _ReflectionTexture("ReflectionTexture", 2D) = "white" {}
        _ReflectionTextureStrength("ReflectionTextureStrength", Float) = 1
        _ReflectionTextureSpeed("ReflectionTextureSpeed", Float) = 0
        [Toggle(_USEGRADIENT_ON)] _UseGradient("UseGradient", Float) = 0
        _GradientPower("GradientPower", Float) = 1.24
        _GradientRemap1("GradientRemap1", Color) = (1,1,1,0)
        _GradientRemap0("GradientRemap0", Color) = (0,0,0,0)
        _RenderTexture("RenderTexture", 2D) = "white" {}
        [HideInInspector] _texcoord( "", 2D ) = "white" {}
        [HideInInspector] __dirty( "", Int ) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"
        }
        Cull Back
        CGINCLUDE
        #include "UnityShaderVariables.cginc"
        #include "UnityPBSLighting.cginc"
        #include "Lighting.cginc"
        #pragma target 3.0
        #pragma shader_feature_local _USEGRADIENT_ON
        struct Input
        {
            float2 uv_texcoord;
        };

        uniform sampler2D _RenderTexture;
        uniform sampler2D _ReflectionTexture;
        uniform float _ReflectionTextureSpeed;
        uniform float _ReflectionTextureStrength;
        uniform float _GradientPower;
        uniform float4 _GradientRemap0;
        uniform float4 _GradientRemap1;


        struct Gradient
        {
            int type;
            int colorsLength;
            int alphasLength;
            float4 colors[8];
            float2 alphas[8];
        };


        Gradient NewGradient(int type, int colorsLength, int alphasLength,
                             float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4,
                             float4 colors5, float4 colors6, float4 colors7,
                             float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4,
                             float2 alphas5, float2 alphas6, float2 alphas7)
        {
            Gradient g;
            g.type = type;
            g.colorsLength = colorsLength;
            g.alphasLength = alphasLength;
            g.colors[0] = colors0;
            g.colors[1] = colors1;
            g.colors[2] = colors2;
            g.colors[3] = colors3;
            g.colors[4] = colors4;
            g.colors[5] = colors5;
            g.colors[6] = colors6;
            g.colors[7] = colors7;
            g.alphas[0] = alphas0;
            g.alphas[1] = alphas1;
            g.alphas[2] = alphas2;
            g.alphas[3] = alphas3;
            g.alphas[4] = alphas4;
            g.alphas[5] = alphas5;
            g.alphas[6] = alphas6;
            g.alphas[7] = alphas7;
            return g;
        }


        float4 SampleGradient(Gradient gradient, float time)
        {
            float3 color = gradient.colors[0].rgb;
            UNITY_UNROLL
            for (int c = 1; c < 8; c++)
            {
                float colorPos = saturate(
                    (time - gradient.colors[c - 1].w) / (0.00001 + (gradient.colors[c].w - gradient.colors[c - 1].w)) *
                    step(c, (float)gradient.colorsLength - 1));
                color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
            }
            #ifndef UNITY_COLORSPACE_GAMMA
            color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g),
                          GammaToLinearSpaceExact(color.b));
            #endif
            float alpha = gradient.alphas[0].x;
            UNITY_UNROLL
            for (int a = 1; a < 8; a++)
            {
                float alphaPos = saturate(
                    (time - gradient.alphas[a - 1].y) / (0.00001 + (gradient.alphas[a].y - gradient.alphas[a - 1].y)) *
                    step(a, (float)gradient.alphasLength - 1));
                alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
            }
            return float4(color, alpha);
        }


        inline half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(0, 0, 0, s.Alpha);
        }

        void surf(Input i, inout SurfaceOutput o)
        {
            float2 temp_cast_0 = ((_Time.y * _ReflectionTextureSpeed)).xx;
            float2 uv_TexCoord17 = i.uv_texcoord + temp_cast_0;
            float4 appendResult8 = (float4(tex2D(_ReflectionTexture, uv_TexCoord17)));
            float4 appendResult10 = (float4(
                (appendResult8 * (0.0 + (_ReflectionTextureStrength - 0.0) * (0.02 - 0.0) / (1.0 - 0.0))).x, 1, 0.0,
                0.0));
            float2 uv_TexCoord3 = i.uv_texcoord * float2(1, -1) + appendResult10.xy;
            float4 break55 = tex2D(_RenderTexture, uv_TexCoord3);
            float4 appendResult27 = (float4(break55.r, break55.g, break55.b, 0.0));
            o.Emission = appendResult27.xyz;
            float4 temp_cast_5 = (1.0).xxxx;
            Gradient gradient37 = NewGradient(0, 2, 2, float4(0, 0, 0, 0.2794079), float4(1, 1, 1, 1), 0, 0, 0, 0, 0, 0,
                                              float2(1, 0), float2(1, 1), 0, 0, 0, 0, 0, 0);
            #ifdef _USEGRADIENT_ON
            float4 staticSwitch43 = (_GradientRemap0 + (SampleGradient(gradient37, (i.uv_texcoord.y * _GradientPower)) -
                float4(0, 0, 0, 0)) * (_GradientRemap1 - _GradientRemap0) / (float4(1, 1, 1, 1) - float4(0, 0, 0, 0)));
            #else
				float4 staticSwitch43 = temp_cast_5;
            #endif
            o.Alpha = staticSwitch43.r;
        }
        ENDCG
        CGPROGRAM
        #pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows
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
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.customPack1.xy = customInputData.uv_texcoord;
                o.customPack1.xy = v.texcoord;
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
                surfIN.uv_texcoord = IN.customPack1.xy;
                float3 worldPos = IN.worldPos;
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
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
}
/*ASEBEGIN
Version=18800
2605;278;1828;996;2497.327;-162.5027;1.3;True;True
Node;AmplifyShaderEditor.CommentaryNode;46;-3019.484,-684.4105;Inherit;False;2192.577;800.8371;Comment;12;3;12;10;5;13;11;8;4;17;16;14;15;Distortion;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2969.483,-384.1354;Float;False;Property;_ReflectionTextureSpeed;ReflectionTextureSpeed;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;14;-2921.382,-536.2355;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-2675.682,-456.9357;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-2475.786,-483.7206;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-2188.945,-278.7507;Float;False;Property;_ReflectionTextureStrength;ReflectionTextureStrength;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-2172.911,-573.264;Inherit;True;Property;_ReflectionTexture;ReflectionTexture;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;8;-1807.558,-461.7617;Inherit;False;FLOAT4;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;-1841.718,-318.4871;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.02;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;45;-2086.164,392.4668;Inherit;False;1619.74;872.2771;Comment;10;44;43;40;41;42;36;39;37;38;35;Opacity Gradient;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;5;-1661.26,-57.6115;Float;False;Constant;_Vector0;Vector 0;2;0;Create;True;0;0;0;False;0;False;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1595.944,-355.8999;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-1348.235,-230.2275;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1929.386,790.6915;Inherit;False;Property;_GradientPower;GradientPower;4;0;Create;True;0;0;0;False;0;False;1.24;1.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-2036.163,595.9568;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1105.594,-165.9844;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,-1;False;1;FLOAT2;0,1;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientNode;37;-1912.13,442.4668;Inherit;False;0;2;2;0,0,0,0.2794079;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-1676.469,662.6243;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-1583.445,1047.124;Inherit;False;Property;_GradientRemap1;GradientRemap1;5;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;56;-667.5653,-251.5219;Inherit;True;Property;_RenderTexture;RenderTexture;7;0;Create;True;0;0;0;False;0;False;-1;7b0c74896ea6ff344a2ffdf20d18c5c8;7b0c74896ea6ff344a2ffdf20d18c5c8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;41;-1588.096,853.3234;Inherit;False;Property;_GradientRemap0;GradientRemap0;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;36;-1482.668,521.5372;Inherit;True;2;0;OBJECT;;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;-1126.531,469.1824;Inherit;False;Constant;_One;One;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;55;-182.2803,-110.486;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TFHCRemapNode;40;-1119.874,803.7108;Inherit;True;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;3;COLOR;0,0,0,0;False;4;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;89.31296,-25.08363;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;43;-789.2987,709.1713;Inherit;False;Property;_UseGradient;UseGradient;3;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;32;433.3258,-23.80483;Float;False;True;-1;2;;0;0;Unlit;MoreMountains/MM2DReflection;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;14;0
WireConnection;16;1;15;0
WireConnection;17;1;16;0
WireConnection;4;1;17;0
WireConnection;8;0;4;0
WireConnection;12;0;11;0
WireConnection;13;0;8;0
WireConnection;13;1;12;0
WireConnection;10;0;13;0
WireConnection;10;1;5;2
WireConnection;3;1;10;0
WireConnection;39;0;35;2
WireConnection;39;1;38;0
WireConnection;56;1;3;0
WireConnection;36;0;37;0
WireConnection;36;1;39;0
WireConnection;55;0;56;0
WireConnection;40;0;36;0
WireConnection;40;3;41;0
WireConnection;40;4;42;0
WireConnection;27;0;55;0
WireConnection;27;1;55;1
WireConnection;27;2;55;2
WireConnection;43;1;44;0
WireConnection;43;0;40;0
WireConnection;32;2;27;0
WireConnection;32;9;43;0
ASEEND*/
//CHKSM=E3715DFFF7D3453E83B992A34567E160F84DCC37