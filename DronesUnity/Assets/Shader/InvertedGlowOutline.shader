Shader "Custom/InvertedGlowOutline"
{
    Properties
    {
        _MainTex ("Основная текстура", 2D) = "white" {}
        _Color ("Цвет объекта", Color) = (1,1,1,1)
        _OutlineColor ("Цвет обводки", Color) = (1,0,0,1)
        _OutlineWidth ("Толщина обводки", Range(0, 0.1)) = 0.02
        _GlowIntensity ("Интенсивность свечения", Range(1, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        // Первый проход: рисуем инвертированную обводку
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float4 objectPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _GlowIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Смещаем вершины наружу по нормали для создания обводки
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                worldPos += worldNormal * _OutlineWidth;
                
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal;
                o.objectPos = v.vertex;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Создаем градиент на основе UV для эффекта свечения
                float distFromCenter = length(i.objectPos.xy);
                float glow = 1 - saturate(distFromCenter * 2);
                
                // Инвертируем UV для создания внутреннего свечения
                float2 invertedUV = float2(1 - i.uv.x, 1 - i.uv.y);
                fixed4 texColor = tex2D(_MainTex, invertedUV);
                
                // Комбинируем цвета
                fixed4 finalColor = _OutlineColor;
                finalColor.a = glow * _GlowIntensity;
                finalColor.rgb *= finalColor.a;
                
                return finalColor;
            }
            ENDCG
        }
        
        // Второй проход: рисуем сам объект
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
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}