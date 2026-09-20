// Unity built-in shader source. Copyright (c) Unity Technologies.
// Modified by [Your Name or Studio].
// Licensed under the Unity Companion License (see https://unity3d.com/legal/licenses/unity-companion-license)

Shader "October/Particles/Standard Unlit Scrolling"
{
    Properties
    {
        _MainTex("Albedo", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DistortionStrength("Strength", Float) = 1.0
        _DistortionBlend("Blend", Range(0.0, 1.0)) = 0.5

        _SoftParticlesNearFadeDistance("Soft Particles Near Fade", Float) = 0.0
        _SoftParticlesFarFadeDistance("Soft Particles Far Fade", Float) = 1.0
        _CameraNearFadeDistance("Camera Near Fade", Float) = 1.0
        _CameraFarFadeDistance("Camera Far Fade", Float) = 2.0

        _ScrollingVelocity ("Scrolling Velocity", Vector) = (1, 0, 0, 0)

        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimStrength("Rim Strength", Range(0, 1)) = 0.5
        _RimGradient("Rim Gradient", Range(0, 1)) = 1
        _RimDirectional("Rim Directional", Range(0, 1)) = 0.1

        // Hidden properties
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _ColorMode ("__colormode", Float) = 0.0
        [HideInInspector] _FlipbookMode ("__flipbookmode", Float) = 0.0
        [HideInInspector] _LightingEnabled ("__lightingenabled", Float) = 0.0
        [HideInInspector] _DistortionEnabled ("__distortionenabled", Float) = 0.0
        [HideInInspector] _EmissionEnabled ("__emissionenabled", Float) = 0.0
        [HideInInspector] _BlendOp ("__blendop", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector] _Cull ("__cull", Float) = 2.0
        [HideInInspector] _SoftParticlesEnabled ("__softparticlesenabled", Float) = 0.0
        [HideInInspector] _CameraFadingEnabled ("__camerafadingenabled", Float) = 0.0
        [HideInInspector] _SoftParticleFadeParams ("__softparticlefadeparams", Vector) = (0,0,0,0)
        [HideInInspector] _CameraFadeParams ("__camerafadeparams", Vector) = (0,0,0,0)
        [HideInInspector] _ColorAddSubDiff ("__coloraddsubdiff", Vector) = (0,0,0,0)
        [HideInInspector] _DistortionStrengthScaled ("__distortionstrengthscaled", Float) = 0.0

        // For switching to URP
        [HideInInspector] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [HideInInspector] _BaseMap("Base Map", 2D) = "white" {}
    }

    Category
    {
        SubShader
        {
            Tags { "RenderType"="Opaque" "IgnoreProjector"="True" "PreviewType"="Plane" "PerformanceChecks"="False" }

            BlendOp [_BlendOp]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            ColorMask RGB

            GrabPass
            {
                Tags { "LightMode" = "Always" }
                "_GrabTexture"
            }

            Pass
            {
                Name "ShadowCaster"
                Tags { "LightMode" = "ShadowCaster" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
                #pragma shader_feature_local _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
                #pragma shader_feature_local _REQUIRE_UV2
                #pragma multi_compile_shadowcaster
                #pragma multi_compile_instancing
                #pragma instancing_options procedural:vertInstancingSetup

                #pragma vertex vertParticleShadowCaster
                #pragma fragment fragParticleShadowCaster

                #include "UnityStandardParticleShadow.cginc"
                ENDCG
            }

            Pass
            {
                Name "SceneSelectionPass"
                Tags { "LightMode" = "SceneSelectionPass" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature_local _ALPHATEST_ON
                #pragma shader_feature_local _REQUIRE_UV2
                #pragma multi_compile_instancing
                #pragma instancing_options procedural:vertInstancingSetup

                #pragma vertex vertEditorPassScrolling
                #pragma fragment fragSceneHighlightPass

                #include "UnityStandardParticleEditor.cginc"

                float2 _ScrollingVelocity;

                void vertEditorPassScrolling(VertexInput v, out VertexOutput o, out float4 opos : SV_POSITION)
                {
                    UNITY_SETUP_INSTANCE_ID(v);

                    opos = UnityObjectToClipPos(v.vertex);

                    #ifdef _FLIPBOOK_BLENDING
                        #ifdef UNITY_PARTICLE_INSTANCING_ENABLED
                            vertInstancingUVs(v.texcoords.xy, o.texcoord, o.texcoord2AndBlend);
                        #else
                            o.texcoord = v.texcoords.xy;
                            o.texcoord2AndBlend.xy = v.texcoords.zw;
                            o.texcoord2AndBlend.z = v.texcoordBlend;
                        #endif
                    #else
                        #ifdef UNITY_PARTICLE_INSTANCING_ENABLED
                            vertInstancingUVs(v.texcoords.xy, o.texcoord);
                            o.texcoord = TRANSFORM_TEX(o.texcoord, _MainTex);
                        #else
                            o.texcoord = TRANSFORM_TEX(v.texcoords.xy, _MainTex);
                        #endif
                    #endif
                    o.color = v.color;

                    o.texcoord = (v.texcoords + _Time.y * _ScrollingVelocity) * _MainTex_ST;
                }
                ENDCG
            }

            Pass
            {
                Name "ScenePickingPass"
                Tags{ "LightMode" = "Picking" }

                BlendOp Add
                Blend One Zero
                ZWrite On
                Cull Off

                CGPROGRAM
                #pragma target 2.5

                #pragma shader_feature_local _ALPHATEST_ON
                #pragma shader_feature_local _REQUIRE_UV2
                #pragma multi_compile_instancing
                #pragma instancing_options procedural:vertInstancingSetup

                #pragma vertex vertEditorPassScrolling
                #pragma fragment fragScenePickingPass

                #include "UnityStandardParticleEditor.cginc"

                float2 _ScrollingVelocity;

                void vertEditorPassScrolling(VertexInput v, out VertexOutput o, out float4 opos : SV_POSITION)
                {
                    UNITY_SETUP_INSTANCE_ID(v);

                    opos = UnityObjectToClipPos(v.vertex);

                    #ifdef _FLIPBOOK_BLENDING
                        #ifdef UNITY_PARTICLE_INSTANCING_ENABLED
                            vertInstancingUVs(v.texcoords.xy, o.texcoord, o.texcoord2AndBlend);
                        #else
                            o.texcoord = v.texcoords.xy;
                            o.texcoord2AndBlend.xy = v.texcoords.zw;
                            o.texcoord2AndBlend.z = v.texcoordBlend;
                        #endif
                    #else
                        #ifdef UNITY_PARTICLE_INSTANCING_ENABLED
                            vertInstancingUVs(v.texcoords.xy, o.texcoord);
                            o.texcoord = TRANSFORM_TEX(o.texcoord, _MainTex);
                        #else
                            o.texcoord = TRANSFORM_TEX(v.texcoords.xy, _MainTex);
                        #endif
                    #endif
                    o.color = v.color;

                    o.texcoord = (v.texcoords + _Time.y * _ScrollingVelocity) * _MainTex_ST;
                }
                ENDCG
            }

            Pass
            {
                Tags { "LightMode"="ForwardBase" }

                CGPROGRAM
                #pragma multi_compile __ SOFTPARTICLES_ON
                #pragma multi_compile_fog
                #pragma target 2.5

                #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
                #pragma shader_feature_local _ _COLOROVERLAY_ON _COLORCOLOR_ON _COLORADDSUBDIFF_ON
                #pragma shader_feature_local _NORMALMAP
                #pragma shader_feature _EMISSION
                #pragma shader_feature_local _FADING_ON
                #pragma shader_feature_local _REQUIRE_UV2
                #pragma shader_feature_local EFFECT_BUMP

                #pragma vertex vertParticleUnlitScrolling
                #pragma fragment fragParticleUnlitScrolling
                #pragma multi_compile_instancing
                #pragma instancing_options procedural:vertInstancingSetup

                #include "UnityStandardParticles.cginc"

                float2 _ScrollingVelocity;
                float4 _RimColor;
                float _RimStrength;
                float _RimGradient;
                float _RimDirectional;

                struct VertexOutputScrolling: VertexOutput
                {
                    float3 normalWorld : TEXCOORD5;
                    float3 viewDir : TEXCOORD6;
                };

                void vertParticleUnlitScrolling (appdata_particles v, out VertexOutputScrolling o)
                {
                    UNITY_SETUP_INSTANCE_ID(v);

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    float4 clipPosition = UnityObjectToClipPos(v.vertex);
                    o.vertex = clipPosition;
                    o.color = v.color;

                    vertColor(o.color);
                    vertTexcoord(v, o);
                    vertFading(o);
                    vertDistortion(o);

                    UNITY_TRANSFER_FOG(o, o.vertex);

                    o.normalWorld = UnityObjectToWorldNormal(v.normal);
                    o.viewDir = WorldSpaceViewDir(v.vertex);
                    o.texcoord = (v.texcoords + _Time.y * _ScrollingVelocity) * _MainTex_ST;
                }

                float3 RimDirLighting(float rimMin, float rimMax, float4 rimColor, float3 viewDirection, float3 worldNormal) 
                {
	                float3 viewDir = normalize(viewDirection);

                    float3 lightDir = _WorldSpaceLightPos0;
                    float myDot = saturate(dot(lightDir, worldNormal));

	                float rimTemp = smoothstep(rimMin, rimMax, (1 - saturate(dot(viewDir, worldNormal))) * lerp(0, 1, myDot));

	                return clamp(rimColor.rgb * rimColor.a * rimTemp, float3(0, 0, 0), float3(100, 100, 100));
                }

                float3 RimLighting(float rimMin, float rimMax, float4 rimColor, float3 viewDirection, float3 worldNormal) 
                {
	                float3 viewDir = normalize(viewDirection);
	                float rimTemp = smoothstep(rimMin, rimMax, 1 - saturate(dot(viewDir, worldNormal)));

	                return clamp(rimColor.rgb * rimColor.a * rimTemp, float3(0, 0, 0), float3(100, 100, 100));
                }

                fixed4 readTexture(sampler2D tex, VertexOutputScrolling IN)
                {
                    fixed4 color = tex2D (tex, IN.texcoord);
                    #ifdef _FLIPBOOK_BLENDING
                    fixed4 color2 = tex2D(tex, IN.texcoord2AndBlend.xy);
                    color = lerp(color, color2, IN.texcoord2AndBlend.z);
                    #endif
                    return color;
                }

                half4 fragParticleUnlitScrolling (VertexOutputScrolling IN) : SV_Target
                {
                    half4 albedo = readTexture (_MainTex, IN);
                    albedo *= _Color;

                    fragColorMode(IN);
                    fragSoftParticles(IN);
                    fragCameraFading(IN);

                    #if defined(_NORMALMAP)
                    float3 normal = normalize (UnpackScaleNormal (readTexture (_BumpMap, IN), _BumpScale));
                    #else
                    float3 normal = float3(0,0,1);
                    #endif

                    #if defined(_EMISSION)
                    half3 emission = readTexture (_EmissionMap, IN).rgb;
                    #else
                    half3 emission = 0;
                    #endif

                    fragDistortion(IN);

                    half4 result = albedo;

                    #if defined(_ALPHAMODULATE_ON)
                    result.rgb = lerp(half3(1.0, 1.0, 1.0), albedo.rgb, albedo.a);
                    #endif

                    result.rgb += emission * _EmissionColor * cameraFade * softParticlesFade;

                    #if !defined(_ALPHABLEND_ON) && !defined(_ALPHAPREMULTIPLY_ON) && !defined(_ALPHAOVERLAY_ON)
                    result.a = 1;
                    #endif

                    #if defined(_ALPHATEST_ON)
                    clip (albedo.a - _Cutoff + 0.0001);
                    #endif

                    float3 viewDir = normalize(IN.viewDir);

                    float rimStrength = 1 - UNITY_ACCESS_INSTANCED_PROP(Props, _RimStrength);
                    float rimGradient = UNITY_ACCESS_INSTANCED_PROP(Props, _RimGradient);
                    float rimDirectional = UNITY_ACCESS_INSTANCED_PROP(Props, _RimDirectional);
                    float4 rimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor);

                    float4 myDirRim = float4(RimDirLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, IN.normalWorld), 0);
                    float4 myRim = float4(RimLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, IN.normalWorld), 0);

                    result += lerp(myRim, myDirRim, rimDirectional) * _RimColor.a;

                    UNITY_APPLY_FOG_COLOR(IN.fogCoord, result, fixed4(0,0,0,0));
                    return result;
                }
                ENDCG
            }
        }
    }

    Fallback "VertexLit"
    CustomEditor "OctoberStudio.StandardParticlesScrollingShaderGUI"
}