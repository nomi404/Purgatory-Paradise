Shader "CharlesGames/HoverShader"
{
    Properties
 {
     [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
     [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
     [PerRendererData] _Hovered ("IsHovered", Float) = 1
     [PerRendererData] _MaxHovered ("MaxHovered", Float) = 0.3
     _Color ("Tint", Color) = (1,1,1,1)
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

     Cull Off
     Lighting Off
     ZWrite Off
     Fog { Mode Off }
     Blend SrcAlpha OneMinusSrcAlpha

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
                float4 color    : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color    : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Hovered;
            half _MaxHovered;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color =  v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col*(1+_Hovered*_MaxHovered);
            }
            ENDCG
        }
    }
}
