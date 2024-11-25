Shader "Custom/gameovershader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DissolveTex("DissolveTex" , 2D) = "white"{}
        _DissolveValue("DissolveValue", range(0, 1)) = 0.0
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
                fixed4 col = tex2D(_MainTex, i.uv);
                float dissolveTex = tex2D(_DissolveTex, i.uv).r;
                float clipValue = dissolveTex - _DissolveValue;
                clip(clipValue);
                
                return col;
            }
            ENDCG
        }
    }
}
