Shader "UGUI/UIWalkLight"
{
	Properties
	{
		[PerRendererData] _MainTex ("MainTex", 2D) = "white" {}
        

		[Header(Stencil)]
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
                [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		
		[Header()]
		_Color("Tint", Color) = (1,1,1,1)
     _LigthTex("WalkLight", 2D) = "white" {}
		_Strength("strength" , float)=1
		_Speed("speed",float)=1
		_RotateAngel("RotateAngel",Range(0,360))=1

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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
                float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				half2 walkLight : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

		   struct v2f
			{
				float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				half2 walkLight :TEXCOORD1;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			sampler2D _LigthTex;
			float4 _LigthTex_TexelSize;

            float4 _Color;
			float _Strength;
			float _Speed;
			float _RotateAngel;

			v2f vert(appdata_t IN)
			{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			OUT.vertex = UnityObjectToClipPos(IN.vertex);
		    OUT.texcoord = IN.texcoord;
			OUT.walkLight =IN.walkLight;
            OUT.color = IN.color;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
               half4 mycolor = tex2D(_MainTex,IN.texcoord) * IN.color * _Color;




			   float angel = _RotateAngel * 3.14159265359 / 180;//角度转弧度，因为正余弦采用弧度

			   //旋转前，改变轴心。不减的话，是以左下角(0,0)为轴心,减去后以(0.5,0.5)为轴心，这才是我们要的旋转效果
			   float2 lightUV = IN.walkLight - float2(0.5,0.5);

			   //这里，应用旋转函数，把UV进行旋转
			   lightUV= float2(lightUV.x * cos(angel) - lightUV.y * sin(angel),lightUV.y * cos(angel) + lightUV.x * sin(angel));
			   
			   //加回偏移
			   lightUV = lightUV + float2(0.5,0.5);

               lightUV.x += frac(_Speed*_Time.y);


			   half4 light = tex2D (_LigthTex,lightUV);

			   float4 alpha = step(1,light.a);

			   float4 fues = lerp(mycolor,mycolor+light*_Strength*mycolor,alpha);

			   fues.a=mycolor.a;


				return fues;
			}
		ENDCG
		}
	}
}
