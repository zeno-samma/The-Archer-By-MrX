Shader "October/October"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        [Toggle]_ReceiveShadow ("Receive Shadows", float) = 1

        _ShadingRampTex("Shading Ramp Texture", 2D) = "white" {}
        _ShadingLows("Shading Lows Color", Color) = (0,0,0,1)
        _ShadingHighs("Shading Highs Color", Color) = (1,1,1,1)

        
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
        Pass
        {
            Tags {"LightMode"="ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normalWorld : TEXCOORD1;
                fixed4 positionWorld : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata_base v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalWorld = UnityObjectToWorldNormal(v.normal);
                o.positionWorld = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = WorldSpaceViewDir(v.vertex);

                o.uv = v.texcoord;
                
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            UNITY_INSTANCING_BUFFER_START(Props)

            UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

            UNITY_DEFINE_INSTANCED_PROP(sampler2D, _ShadingRampTex)
            UNITY_DEFINE_INSTANCED_PROP(float4, _ShadingLows)
            UNITY_DEFINE_INSTANCED_PROP(float4, _ShadingHighs)

            UNITY_DEFINE_INSTANCED_PROP(float4, _RimColor)
            UNITY_DEFINE_INSTANCED_PROP(float, _RimStrength)
            UNITY_DEFINE_INSTANCED_PROP(float, _RimGradient)
            UNITY_DEFINE_INSTANCED_PROP(float, _RimDirectional)

            UNITY_DEFINE_INSTANCED_PROP(float, _ReceiveShadow)

            UNITY_DEFINE_INSTANCED_PROP(float, _SpecularStrength)
            UNITY_DEFINE_INSTANCED_PROP(float, _SpecularGradient)
            UNITY_DEFINE_INSTANCED_PROP(float4, _SpecularColor)

            UNITY_INSTANCING_BUFFER_END(Props)

            float4 GetToon(float4 positionWS, float3 normalWS, float shadow)
            {
                float receiveShadows = UNITY_ACCESS_INSTANCED_PROP(Props, _ReceiveShadow);
                float4 shadingLows = UNITY_ACCESS_INSTANCED_PROP(Props, _ShadingLows);
                float4 shadingHighs = UNITY_ACCESS_INSTANCED_PROP(Props, _ShadingHighs);

                float3 lightDir = _WorldSpaceLightPos0;
                float tempToon = (dot(normalWS, lightDir) + 1) / 2 * (receiveShadows ? saturate(shadow) : 1);

                float3 rampTemp =  lerp(shadingLows, shadingHighs, tempToon).rgb;
                float3 rampTextTemp = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props,_ShadingRampTex), half2(tempToon, 0.5)).rgb;

                float3 toon = rampTemp * rampTextTemp;

                return float4(toon, 1);
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

            float3 Specular(float specularMin, float specularMax, float4 specularColor, float3 viewDirection, float3 worldNormal)
            {
                float3 viewDir = normalize(viewDirection);

                float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
                float NdotH = saturate(dot(worldNormal, halfVector));

                //float glossiness = UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness) * 30;
                float NdotL = saturate(dot(_WorldSpaceLightPos0, worldNormal));
                //float specularIntensity = pow(NdotH * smoothstep(0, 0.01, NdotL), glossiness * glossiness);

                float specularTemp = saturate(NdotH * NdotL);
                specularTemp = smoothstep(specularMin, specularMax, specularTemp);

                return clamp(specularColor.rgb * specularColor.a * specularTemp, float3(0, 0, 0), float3(100, 100, 100));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                fixed4 col = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex), i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                fixed shadow = SHADOW_ATTENUATION(i);

                col.rgb *= GetToon(i.positionWorld, i.normalWorld, shadow) * _LightColor0.rgb;
                
                float3 viewDir = normalize(i.viewDir);

                float specularStrength = 1 - UNITY_ACCESS_INSTANCED_PROP(Props, _SpecularStrength);
                float specularGradient = UNITY_ACCESS_INSTANCED_PROP(Props, _SpecularGradient);
                float4 specularColor = UNITY_ACCESS_INSTANCED_PROP(Props, _SpecularColor);

                float4 specularNew = float4(Specular(specularStrength - specularGradient, specularStrength + specularGradient, specularColor, viewDir, i.normalWorld), 0);
                col += specularNew;

                float rimStrength = 1 - UNITY_ACCESS_INSTANCED_PROP(Props, _RimStrength);
                float rimGradient = UNITY_ACCESS_INSTANCED_PROP(Props, _RimGradient);
                float rimDirectional = UNITY_ACCESS_INSTANCED_PROP(Props, _RimDirectional);
                float4 rimColor = UNITY_ACCESS_INSTANCED_PROP(Props, _RimColor);

                float4 myDirRim = float4(RimDirLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, i.normalWorld), 0);
                float4 myRim = float4(RimLighting(rimStrength - rimGradient, rimStrength + rimGradient, rimColor, viewDir, i.normalWorld), 0);

                col += lerp(myRim, myDirRim, rimDirectional) * _RimColor.a;

                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}