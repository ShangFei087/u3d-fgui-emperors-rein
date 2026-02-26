Shader "S_Game_Effects/Scroll2TexBend.add" {
Properties {
	[WrapMode]_MainTex1_Wrap("Tex1 Wrap Mode", Float) = 0
	_MainTex1 ("Tex1(RGB)", 2D) = "white" {}
	[WrapMode]_MainTex2_Wrap("Tex2 Wrap Mode", Float) = 0
	_MainTex2 ("Tex2(RGB)", 2D) = "white" {}
	_ScrollX ("Tex1 speed X", Float) = 1.0
	_ScrollY ("Tex1 speed Y", Float) = 0.0
	_Scroll2X ("Tex2 speed X", Float) = 1.0
	_Scroll2Y ("Tex2 speed Y", Float) = 0.0
	_Color("Color", Color) = (1,1,1,1)
	_UVXX("UVXX", vector)=(0.3,1,1,1)	
	_MMultiplier ("Layer Multiplier", Float) = 2.0
	[Header(Render State)]
	_PolygonOffset("Polygon Offset", float) = 0
}

	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
	Blend SrcAlpha One, Zero One
	Cull Off
	Offset [_PolygonOffset], [_PolygonOffset]
	ZWrite Off
	LOD 100
	
	
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex1;
	sampler2D _MainTex2;

	float4 _MainTex1_ST;
	float4 _MainTex2_ST;
	float _MainTex1_Wrap;
	float _MainTex2_Wrap;
	
	float _ScrollX;
	float _ScrollY;
	float _Scroll2X;
	float _Scroll2Y;
	fixed _MMultiplier;
	float4 _UVXX;
	fixed4 _Color;

	struct v2f {
		float4 pos : SV_POSITION;
		float4 uv : TEXCOORD0;
		fixed4 color : COLOR;
	};

	float2 WrapMode(float2 uv, float mode) // mode 0=Repeat 1=Clamp
	{
		return lerp(uv, saturate(uv), mode);
	}

#define TRANSFORM_TEX2(tex1, name1, tex2, name2) (float4(tex1.xy, tex2.xy) * float4(name1##_ST.xy, name2##_ST.xy) + float4(name1##_ST.zw, name2##_ST.zw))

	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX2(v.texcoord, _MainTex1, v.texcoord, _MainTex2);
		o.uv += frac(float4(_ScrollX, _ScrollY, _Scroll2X, _Scroll2Y) * _Time.x);
		o.color = _MMultiplier * _Color * v.color;
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
			i.uv.xy = WrapMode(i.uv.xy, _MainTex1_Wrap);
			fixed4 tex = tex2D(_MainTex1, i.uv.xy);
			
			float2 uv2 = i.uv.zw + tex.r * _UVXX.x;
			uv2 = WrapMode(uv2, _MainTex2_Wrap);
			fixed4 tex2 = tex2D(_MainTex2, uv2);
			
			o = tex * tex2 * i.color;
			//o.a = 	dot(o.rgb, float3(0.3,0.59,0.11));
			return o;
		}
		ENDCG 
	}
}
}
