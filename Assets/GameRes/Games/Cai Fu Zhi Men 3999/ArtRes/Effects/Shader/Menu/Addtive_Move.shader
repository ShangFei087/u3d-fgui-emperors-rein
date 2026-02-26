Shader "S_Game_Effects/Addtive_Move" {
//Shader "S_Game_Effects/Scroll2TexBend" {
Properties {
	[WrapMode]_MainTex_Wrap("Tex1 Wrap Mode", Float) = 0
	_MainTex ("Tex1(RGB)", 2D) = "white" {}
	//_MainTex2 ("Tex2(RGB)", 2D) = "white" {}
	_ScrollX ("Tex1 speed X", Float) = 1.0
	_ScrollY ("Tex1 speed Y", Float) = 0.0
	//_Scroll2X ("Tex2 speed X", Float) = 1.0
	//_Scroll2Y ("Tex2 speed Y", Float) = 0.0
	//_Color("Color", Color) = (1,1,1,1)
	//_UVXX("UVXX", vector)=(0.3,1,1,1)	
	_MMultiplier ("Layer Multiplier", Float) = 2.0
	[Header(Render State)]
	_PolygonOffset("Polygon Offset", float) = 0
}

	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	//Tags { "Queue"="Overlay" }
	
	Blend SrcAlpha One, Zero One
	Cull Off
	Offset [_PolygonOffset], [_PolygonOffset]
	ZWrite Off
	
	LOD 200
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	//sampler2D _MainTex2;

	float4 _MainTex_ST;
//	float4 _MainTex2_ST;

	float _MainTex_Wrap;
	
	float _ScrollX;
	float _ScrollY;
//	float _Scroll2X;
//	float _Scroll2Y;
	float _MMultiplier;
//	float4 _UVXX;
//	float4 _Color;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		fixed4 color : TEXCOORD1;
	};

	float2 WrapMode(float2 uv, float mode) // mode 0=Repeat 1=Clamp
	{
		return lerp(uv, saturate(uv), mode);
	}
	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.uv += frac(float2(_ScrollX, _ScrollY) * _Time.x);
		
		o.color = _MMultiplier  * v.color;
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		fixed4 frag (v2f i) : COLOR
		{
			fixed4 o;
			i.uv = WrapMode(i.uv.xy, _MainTex_Wrap);
			fixed4 tex = tex2D (_MainTex, i.uv.xy);
			
			o = tex * i.color;

			return o;
		}
		ENDCG 
	}	
}
}
