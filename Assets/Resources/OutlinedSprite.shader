Shader "Custom/OutlinedSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        
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
            float _OutlineWidth;
            float4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texColor = tex2D(_MainTex, i.uv);
                
                half4 outlineColor = _OutlineColor;
                outlineColor.a *= texColor.a; // 기존 알파 값을 유지

                half dist = distance(i.uv, float2(0.5, 0.5));
                half4 finalColor = texColor;
                
                if (dist > (0.5 - _OutlineWidth))
                {
                    finalColor = outlineColor;
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}
