Shader "Custom/RadarCones_AlongMesh"
{
    Properties
    {
        [Header(Main Settings)]
        _Color ("Base Color", Color) = (0, 0, 1, 0.2) // Синий полупрозрачный
        _PulseColor ("Pulse Color", Color) = (0, 0.8, 1, 1) // Ярко-синий для полос
        _GlowIntensity ("Pulse Intensity", Float) = 2.0
        
        [Header(Radar Pulse Along Cone)]
        _Speed ("Pulse Speed", Float) = 1.0
        _Frequency ("Pulse Frequency", Float) = 3.0 // Количество полос
        _Width ("Pulse Width", Float) = 0.15 // Толщина полос
        
        [Header(Transparency)]
        _Alpha ("Base Alpha", Range(0, 1)) = 0.15
        _FresnelPower ("Edge Glow Power", Range(0, 5)) = 1.5
    }
    
    SubShader
    {
        Tags { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off // Рисуем обе стороны конуса
        
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 positionOS : TEXCOORD3;
            };
            
            // Переменные
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half4 _PulseColor;
            half _GlowIntensity;
            half _Speed;
            half _Frequency;
            half _Width;
            half _Alpha;
            half _FresnelPower;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.uv = IN.uv;
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // ДЛЯ КОНУСА: Импульсы идут вдоль боковых граней
                // Используем высоту (Y в локальных координатах) для движения вдоль конуса
                
                // Нормализуем высоту в диапазон 0-1
                // Предполагаем, что конус от Y=0 (основание) до Y=1 (вершина)
                // Для стандартного конуса Unity высота обычно около 1-2 единиц
                float height = IN.positionOS.y;
                
                // Масштабируем высоту под частоту
                float time = _Time.y * _Speed;
                
                // Создаем движущиеся полосы вдоль высоты
                // Для движения от вершины к основанию вычитаем время
                float pulseValue = height * _Frequency - time;
                
                // Получаем дробную часть для создания повторяющихся полос
                float fracPos = frac(pulseValue);
                
                // Создаем импульс (пик в центре каждой полосы)
                // Используем треугольную волну для плавных переходов
                float wave = 1.0 - abs(fracPos - 0.5) * 2.0;
                
                // Преобразуем в резкие полосы с контролируемой шириной
                half intensity = smoothstep(1.0 - _Width, 1.0, wave);
                
                // Усиливаем интенсивность для яркости
                intensity = pow(intensity, 1.5);
                
                // ДОПОЛНИТЕЛЬНО: Добавляем круговые полосы (опционально)
                // Радиус для круговых сечений (кольца вокруг конуса)
                float radius = length(IN.positionOS.xz);
                float maxRadius = lerp(0.5, 0.0, height); // Максимальный радиус на данной высоте
                float radialFactor = saturate(radius / maxRadius);
                
                // Модулируем интенсивность так, чтобы полосы были сильнее по краям
                intensity *= radialFactor;
                
                // FRESNEL для свечения по краям
                float3 viewDirWS = normalize(GetCameraPositionWS() - IN.positionWS);
                float fresnel = pow(1.0 - saturate(dot(viewDirWS, IN.normalWS)), _FresnelPower);
                
                // Базовый цвет
                half4 col = _Color;
                
                // Добавляем импульсы
                half3 pulseRGB = _PulseColor.rgb * intensity * _GlowIntensity;
                col.rgb += pulseRGB;
                
                // Добавляем свечение по краям
                col.rgb += _PulseColor.rgb * fresnel * 0.3;
                
                // Прозрачность: усиливаем там, где проходят импульсы
                col.a = _Alpha + intensity * _PulseColor.a * 0.7;
                col.a = saturate(col.a);
                
                return col;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
}