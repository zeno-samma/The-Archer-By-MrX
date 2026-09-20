Shader "Unlit/UIMaskedShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _FlipAlpha("Flip Alpha", int) = 0
        _MaxAlphaV("Max Alpha V", float) = 1
        _MinAlphaV("Min Alpha V", float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 localPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            bool _FlipAlpha;
            float _MaxAlphaV;
            float _MinAlphaV;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                o.localPos = v.vertex.xy; // Local position inside rect (centered)
                return o;
            }

            float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
            {
                return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
            }

            float Lerp(float a, float b, float t)
            {
                return a + (b - a) * t;
            }

            float InverseLerp(float a, float b, float value)
            {
                return (value - a) / (b - a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainCol = tex2D(_MainTex, i.uv) * i.color;

                float alpha = saturate(InverseLerp(_MinAlphaV, _MaxAlphaV, i.localPos.y));

                if(_FlipAlpha) alpha = 1 - alpha;

                mainCol.a *= alpha;
                return mainCol;
            }
            ENDCG
        }
    }
}
