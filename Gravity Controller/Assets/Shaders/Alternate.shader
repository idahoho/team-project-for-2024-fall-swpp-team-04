Shader "Custom/Alternate"
{
    Properties
    {
        _Color1 ("Color1", Color) = (1,1,1,1) // Default
        _Color2 ("Color2", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Progress ("Progress", Range(0,1)) = 0.0
        
        //Emission
        _Emission ("Emission", float) = 0.0
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        
        //Dissolve
        _Noise ("Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", float) = 1.0
        _DissolveProgress ("Dissolve Progress", Range(0,1)) = 0.0
        _DissolveDepth ("Dissolve Depth", Range(0,1)) = 0.0
        _DissolveColor ("Dissolve Color", Color) = (1,1,1,1)
    }
    SubShader
    {   
        //Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
        Tags { "Queue"="Transparent" "RenderType"="TransparentCutout" }
        Lighting On
        SeparateSpecular On
        Blend SrcAlpha OneMinusSrcAlpha
        //Cull Back
        //ZWrite On
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows keepalpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma shader_feature _RENDERING_CUTOUT
        #pragma shader_feature _SMOOTHNESS_ALBEDO

        sampler2D _MainTex;
        sampler2D _Noise;
        
        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color1;
        fixed4 _Color2;
        fixed _Progress;

        fixed _Emission;
        fixed4 _EmissionColor;
        
        fixed _NoiseScale;
        fixed _DissolveProgress;
        fixed _DissolveDepth;
        fixed4 _DissolveColor;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 noise = tex2D (_Noise, IN.uv_MainTex / _NoiseScale);
            //fixed linearNoise = mul (noise.rgb, (1,1,1)) / 3;
            fixed linearNoise = noise.r;
            
            //fixed t = (2 * _SinTime.w * _CosTime.w + 1)/2;
            fixed t = _Progress;
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * (t * _Color2 + (1 - t) * _Color1);
            o.Albedo = linearNoise > _DissolveProgress ? c.rgb : linearNoise > _DissolveProgress - _DissolveDepth ? _DissolveColor : (0,0,0);
            // Metallic and smoothness come from slider variables
            o.Metallic = linearNoise > _DissolveProgress - _DissolveDepth ? _Metallic : 0;
            o.Smoothness = linearNoise > _DissolveProgress - _DissolveDepth ? _Glossiness : 0;
            o.Emission = linearNoise > _DissolveProgress ? ((1 - t) * _Emission + t) * _EmissionColor : linearNoise > _DissolveProgress - _DissolveDepth ? _DissolveColor : 0;
            o.Alpha = linearNoise > _DissolveProgress - _DissolveDepth ? 1 : 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}