// Retrato "2.5D" para UI: relieve calculado en pantalla a partir de la propia imagen (alfa suavizado = volumen,
// luminancia = detalle) y una lámpara virtual de interrogatorio. No hay mapas nuevos ni se toca el archivo
// original; la gradación (desaturar, tinte, brillo) es la misma que Detective/UI/Desaturate.
// Basado en UI/Default (soporta máscaras, RectMask2D y el color del Graphic).
Shader "Detective/UI/LitPortrait"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0, 1)) = 1
        _Grade ("Grade Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0, 2)) = 1

        _LightDir ("Light Direction (xy pantalla, z hacia el espectador)", Vector) = (-0.5, 0.6, 0.6, 0)
        _LightColor ("Light Color", Color) = (1, 0.9, 0.75, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.45
        _Relief ("Relief", Range(0, 8)) = 3
        _Radius ("Sample Radius (texels)", Range(1, 24)) = 6
        _RimColor ("Rim Color", Color) = (0.55, 0.65, 0.8, 1)
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.35
        _Falloff ("Lamp Falloff (arriba claro, abajo oscuro)", Range(0, 1)) = 0.35

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _Saturation;
            fixed4 _Grade;
            float _Brightness;
            float4 _LightDir;
            fixed4 _LightColor;
            float _Ambient;
            float _Relief;
            float _Radius;
            fixed4 _RimColor;
            float _RimStrength;
            float _Falloff;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Altura: el alfa da el volumen (bordes bajos, centro alto) y la luminancia el detalle fino
            half Height(float2 uv)
            {
                half4 c = tex2D(_MainTex, uv);
                return c.a * 0.85 + dot(c.rgb, half3(0.299, 0.587, 0.114)) * c.a * 0.15;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;

                half luma = dot(color.rgb, half3(0.299, 0.587, 0.114));
                color.rgb = lerp(luma.xxx, color.rgb, _Saturation) * _Grade.rgb;

                // Volumen: gradiente de la altura a escala ancha (el cuerpo se curva hacia la lámpara)
                float2 d1 = _MainTex_TexelSize.xy * _Radius;
                float2 d2 = d1 * 4.0;
                half gx = Height(IN.texcoord + float2(d2.x, 0)) - Height(IN.texcoord - float2(d2.x, 0));
                half gy = Height(IN.texcoord + float2(0, d2.y)) - Height(IN.texcoord - float2(0, d2.y));
                half3 n = normalize(half3(-gx * _Relief, -gy * _Relief, 1));

                half3 l = normalize(_LightDir.xyz);
                half diffuse = saturate(dot(n, l));
                // Lámpara cenital: más luz arriba (la cara) que abajo
                half lamp = lerp(1.0 - _Falloff, 1.0, saturate(IN.texcoord.y));
                half3 lit = color.rgb * (_Ambient + _LightColor.rgb * diffuse * (1.0 - _Ambient)) * lamp;

                // Contraluz frío: solo en el filo de la silueta (alfa a escala fina) que da la espalda a la lámpara
                half ax = tex2D(_MainTex, IN.texcoord + float2(d1.x, 0)).a - tex2D(_MainTex, IN.texcoord - float2(d1.x, 0)).a;
                half ay = tex2D(_MainTex, IN.texcoord + float2(0, d1.y)).a - tex2D(_MainTex, IN.texcoord - float2(0, d1.y)).a;
                half2 inward = half2(ax, ay);
                half edge = saturate(length(inward));
                half rim = edge * saturate(dot(normalize(inward + 1e-4), l.xy));
                lit += _RimColor.rgb * rim * _RimStrength * smoothstep(0.85, 1.0, color.a);

                color.rgb = lit * _Brightness;
                color *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}
