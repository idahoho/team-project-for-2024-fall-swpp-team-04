Shader "Custom/gameovershader"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _DissolveTex("DissolveTex" , 2D) = "white"{}
        _DissolveScaleOffset("DissolveScaleOffset", vector) = (1,1,0,0)
        _DissolveValue("DissolveValue", range(0, 1)) = 0.0

        [HDR] _EdgeGlowColor("EdgeGlowColor", color) = (1,1,1,1)
        _EdgeGlowMaskValue("EdgeGlowMaskValue", float) = 0.0
        _EdgeGlowMaskBlur("EdgeGlowMaskBlur", float) = 0.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            sampler2D _DissolveTex;
            float _DissolveValue;
            float4 _DissolveScaleOffset;
            float4 _EdgeGlowColor;
            float _EdgeGlowMaskValue;
            float _EdgeGlowMaskBlur;
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float dissolveTex = tex2D(_DissolveTex, i.uv * _DissolveScaleOffset.xy + _DissolveScaleOffset.zw).r;
                float clipValue = dissolveTex - _DissolveValue;

                float edgeGlowMask = 1.0 - smoothstep(_EdgeGlowMaskValue, _EdgeGlowMaskBlur + _EdgeGlowMaskValue, clipValue);
                col.rgb += edgeGlowMask * _EdgeGlowColor;
                clip(clipValue);
                
                return col;
            }
            ENDCG
        }
    }
}
