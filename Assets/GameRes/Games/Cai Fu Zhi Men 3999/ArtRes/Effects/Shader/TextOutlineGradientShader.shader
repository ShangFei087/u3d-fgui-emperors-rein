Shader "Custom/TextOutlineGradient"  
{  
    Properties  
    {  
        _MainTex ("Font Texture", 2D) = "white" {}  
        _OutlineColor1 ("Outline Color 1", Color) = (0,0,0,1)  
        _OutlineColor2 ("Outline Color 2", Color) = (1,0,0,1)  
        _OutlineWidth ("Outline Width", Float) = 0.1  
    }  
    SubShader  
    {  
        Tags { "RenderType"="Transparent" }  
        LOD 200  
        
        Blend SrcAlpha OneMinusSrcAlpha  
        ZWrite Off  

        Pass  
        {  
            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
            #include "UnityCG.cginc"  

            struct appdata_t {  
                float4 vertex : POSITION;  
                float2 uv : TEXCOORD0;  
            };  

            struct v2f {  
                float2 uv : TEXCOORD0;  
                float4 vertex : SV_POSITION;  
                float4 outlineColor : COLOR;  
            };  

            sampler2D _MainTex;  
            float4 _MainTex_ST;  
            float4 _OutlineColor1;  
            float4 _OutlineColor2;  
            float _OutlineWidth;  

            v2f vert (appdata_t v) {  
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);  
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);  
                return o;  
            }  

            fixed4 frag (v2f i) : SV_Target {  
                fixed4 texColor = tex2D(_MainTex, i.uv);  

                // 以 UV 坐标的 y 分量来插值描边颜色  
                float outlineFraction = i.uv.y > 0.5 ? 1.0 : 0.0;  
                fixed4 outlineColor = lerp(_OutlineColor1, _OutlineColor2, outlineFraction);  

                // 如果文本透明度小于一定值，显示描边颜色  
                if (texColor.a < 0.1) {  
                    return outlineColor;  
                }  

                return texColor;  
            }  
            ENDCG  
        }  
    }  
}  