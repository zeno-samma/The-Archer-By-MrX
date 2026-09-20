Shader "October/October URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        _ReceiveShadow ("Receive Shadows", float) = 1

        _ShadingRampTex("Shading Ramp Texture", 2D) = "white" {}
        _ShadingLows("Shading Lows Color", Color) = (0,0,0,1)
        _ShadingHighs("Shading Highs Color", Color) = (1,1,1,1)

        [HDR]
        _SpecularStrength("Specular Strength", Range(0, 1)) = 0.1
        _SpecularGradient("Specular Gradient", Range(0, 1)) = 0
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 0.4)

        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimStrength("Rim Strength", Range(0, 1)) = 0.5
        _RimGradient("Rim Gradient", Range(0, 1)) = 1
        _RimDirectional("Rim Directional", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Cull back

        Tags { 
            "RenderType" = "Opaque" 
    
            "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward"

            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"

            "UniversalMaterialType" = "Lit" 
            "ShaderModel" = "4.5"
        }

        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                float4 color : COLOR0;

                float3 normal :NORMAL;
                float4 tan :TANGENT;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 normalWorld : TEXCOORD1;
                half4 positionWorld : TEXCOORD2;
                float3 viewDir : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;

                o.pos = TransformObjectToHClip(v.vertex.xyz);

                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal, v.tan);
                o.normalWorld = normalInputs.normalWS;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.positionWorld = float4(positionInputs.positionWS, 1);

                o.viewDir = GetWorldSpaceViewDir(o.positionWorld.xyz);

                o.uv = v.uv;

                return o;
            }

            sampler2D _MainTex;
            float4 _Color;

            sampler2D _ShadingRampTex;
            float4 _ShadingLows;
            float4 _ShadingHighs;

            float4 _RimColor;
            float _RimStrength;
            float _RimGradient;
            float _RimDirectional;

            float _ReceiveShadow;

            float _SpecularStrength;
            float _SpecularGradient;
            float4 _SpecularColor;

            float4 GetToon(float4 positionWS, float3 normalWS, float shadow)
            {
                float receiveShadows = _ReceiveShadow;
                float4 shadingLows = _ShadingLows;
                float4 shadingHighs = _ShadingHighs;

                float4 shadowCoord = TransformWorldToShadowCoord(positionWS.xyz);
	            Light mainLight = GetMainLight(shadowCoord);

                float3 lightDir = mainLight.direction;
                float tempToon = (dot(normalWS, lightDir) + 1) / 2 * (receiveShadows ? saturate(shadow) : 1);

                float3 rampTemp =  lerp(shadingLows, shadingHighs, tempToon).rgb;
                float3 rampTextTemp = tex2D(_ShadingRampTex, half2(tempToon, 0.5)).rgb;

                float3 toon = rampTemp * rampTextTemp;

                return float4(toon, 1);
            }

            float3 RimDirLighting(float rimMin, float rimMax, float4 rimColor, float3 viewDirection, float3 worldNormal, float4 positionWS) 
            {
	            float3 viewDir = normalize(viewDirection);

                float4 shadowCoord = TransformWorldToShadowCoord(positionWS.xyz);
	            Light mainLight = GetMainLight(shadowCoord);

                float3 lightDir = mainLight.direction;
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

            float3 Specular(float specularMin, float specularMax, float4 specularColor, float3 viewDirection, float3 worldNormal, float4 positionWS)
            {
                float3 viewDir = normalize(viewDirection);

                float4 shadowCoord = TransformWorldToShadowCoord(positionWS.xyz);
	            Light mainLight = GetMainLight(shadowCoord);

                float3 lightDir = mainLight.direction;

                float3 halfVector = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(worldNormal, halfVector));

                //float glossiness = UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness) * 30;
                float NdotL = saturate(dot(lightDir, worldNormal));
                //float specularIntensity = pow(NdotH * smoothstep(0, 0.01, NdotL), glossiness * glossiness);

                float specularTemp = saturate(NdotH * NdotL);
                specularTemp = smoothstep(specularMin, specularMax, specularTemp);

                return clamp(specularColor.rgb * specularColor.a * specularTemp, float3(0, 0, 0), float3(100, 100, 100));
            }

            float4 Applyfog(float4 color, float3 positionWS)
            {
                float4 inColor = color;
  
                #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
                float viewZ = -TransformWorldToView(positionWS).z;
                float nearZ0ToFarZ = max(viewZ - _ProjectionParams.y, 0);
                float density = 1.0f - ComputeFogIntensity(ComputeFogFactorZ0ToFar(nearZ0ToFarZ));

                color = lerp(color, unity_FogColor,  density);

                #endif

                return color;
            }

            float4 frag(v2f i) : COLOR
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;

                half4 shadowCoord = TransformWorldToShadowCoord(i.positionWorld.xyz);
	            Light mainLight = GetMainLight(shadowCoord);
                float shadow = mainLight.shadowAttenuation;

                col.rgb *= GetToon(i.positionWorld, i.normalWorld, shadow).rgb * mainLight.color.rgb;

                float3 viewDir = normalize(i.viewDir);

                float specularStrength = 1 - _SpecularStrength;
                float specularGradient = _SpecularGradient;
                float4 specularColor = _SpecularColor;

                float4 specularNew = float4(Specular(specularStrength - specularGradient, specularStrength + specularGradient, specularColor, viewDir, i.normalWorld, i.positionWorld), 0);
                col += specularNew;

                float rimStrength = 1 - _RimStrength;
                float rimGradient = _RimGradient;
                float rimDirectional = _RimDirectional;
                float4 rimColor = _RimColor;

                float4 myDirRim = float4(RimDirLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, i.normalWorld, i.positionWorld), 0);
                float4 myRim = float4(RimLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, i.normalWorld), 0);

                col += lerp(myRim, myDirRim, rimDirectional) * _RimColor.a;

                col = Applyfog(col, i.positionWorld.xyz);

                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull back

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }
}