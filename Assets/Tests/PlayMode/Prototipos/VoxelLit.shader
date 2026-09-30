// Vóxeles del retrato: color por vértice, luz de lámpara (dirección fija en el material) y ambiente.
// Sin dependencia de las luces del pipeline (el renderizador 2D no ilumina mallas 3D).
Shader "Detective/VoxelLit"
{
    Properties
    {
        _LightDir ("Light Direction", Vector) = (-0.5, 0.6, -0.6, 0)
        _LightColor ("Light Color", Color) = (1, 0.9, 0.75, 1)
        _Ambient ("Ambient", Range(0, 1)) = 0.45
        _Saturation ("Saturation", Range(0, 1)) = 1
        _Grade ("Grade Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        ZWrite On

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; fixed4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float3 normal : TEXCOORD0; };

            float4 _LightDir;
            fixed4 _LightColor;
            float _Ambient;
            float _Saturation;
            fixed4 _Grade;
            float _Brightness;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 c = i.color.rgb;
                half luma = dot(c, half3(0.299, 0.587, 0.114));
                c = lerp(luma.xxx, c, _Saturation) * _Grade.rgb;
                half d = saturate(dot(normalize(i.normal), normalize(-_LightDir.xyz)));
                c *= (_Ambient + _LightColor.rgb * d * (1.0 - _Ambient)) * _Brightness;
                return fixed4(c, 1);
            }
        ENDCG
        }
    }
}
