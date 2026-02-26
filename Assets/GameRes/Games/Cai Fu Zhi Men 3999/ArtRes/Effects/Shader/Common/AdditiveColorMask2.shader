Shader "S_Game_Effects/AdditiveColorMask2" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
//	_AlphaTex("_AlphaTex", 2D) = "white" {}
	_ColorMaskTex ("Color Mask Texture", 2D) = "white" {}
	_Level("Brightness", Float) = 1
}

CGINCLUDE

	#include "UnityCG.cginc"
	
	uniform sampler2D _MainTex;
	uniform sampler2D _ColorMaskTex;
	uniform float4 _MainTex_ST;
	uniform float4 _ColorMaskTex_ST;
	half4 _TintColor;
	half _Level;


	struct appdata {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		half4 color : COLOR;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float2	uv : TEXCOORD0;
		float2	uv2 : TEXCOORD1;
		half4	color : TEXCOORD2;
	};	

	v2f vert (appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos (v.vertex);		
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv2 = TRANSFORM_TEX(v.texcoord, _ColorMaskTex);
		o.color = v.color * _TintColor;
		return o;
	}
	
	half4 frag (v2f i) : COLOR
	{
		half4 color = tex2D(_MainTex, i.uv.xy);
		half4 texcol = color * tex2D( _ColorMaskTex, i.uv2 ) * i.color * _Level;
		return texcol;
	}

	ENDCG
	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One, Zero One
	Cull Off
	ZWrite Off

    Pass {

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	
	ENDCG
    }
}
Fallback Off
}