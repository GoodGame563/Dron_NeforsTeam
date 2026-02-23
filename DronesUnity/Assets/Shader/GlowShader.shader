Shader "Custom/GlowShader"
{
    Properties
    {
        _MainTex ("Альбедо (RGB)", 2D) = "white" {}
        _GlowColor ("Цвет свечения", Color) = (1,1,1,1)
        _GlowIntensity ("Интенсивность свечения", Range(0, 5)) = 1
        _GlowTex ("Текстура свечения", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _GlowTex;
        float4 _GlowColor;
        float _GlowIntensity;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_GlowTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Основной цвет
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
            // Цвет свечения из текстуры
            fixed4 glow = tex2D (_GlowTex, IN.uv_GlowTex);
            
            // Эмиссивный канал (свечение)
            o.Emission = glow.rgb * _GlowColor.rgb * _GlowIntensity;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}