// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "tongyong"
{
	Properties
	{
		_MainTex("主帖图", 2D) = "white" {}
		[HDR]_MainColor("主帖图颜色", Color) = (0,0,0,0)
		_MainTexUVSpeed("主贴图UV流动", Vector) = (0,0,0,0)
		_MainTexRotate("主帖图旋转", Float) = 0
		[KeywordEnum(Off,On)] _MainTexPolar("主帖图极坐标开关", Float) = 0
		_MainTexPolarUV("主帖图极坐标UV", Vector) = (1,1,0,0)
		_MainTexXulieSpeed("主帖图序列速度", Float) = 0
		_MainTexXulieKongzhi("主帖图序列排列", Vector) = (0,0,0,0)
		[KeywordEnum(Off,On)] _MainTexXulie("主帖图序列开关", Float) = 0
		_MainTexXuliePlay("主帖图序列滑杆", Float) = 0
		_DisrtortTex1("扭曲贴图1", 2D) = "white" {}
		_DisrtortIntensity1("扭曲强度1", Float) = 0
		_DisrtortUVSpeed1("扭曲UV流动1", Vector) = (0,0,0,0)
		_DisrtortRotate1("扭曲旋转1", Float) = 0
		[KeywordEnum(Off,On)] _DisrtortPolar1("扭曲极坐标开关1", Float) = 0
		_DisrtortPolarUV1("扭曲极坐标UV1", Vector) = (1,1,0,0)
		_MaskTex1("遮罩贴图1", 2D) = "white" {}
		_MaskTexUVSpeed3("遮罩UV流动1", Vector) = (0,0,0,0)
		_MaskTexRotate1("遮罩旋转1", Float) = 0
		[KeywordEnum(Off,On)] _MaskTexPolar1("遮罩极坐标开关1", Float) = 0
		_MaskTexPolarUV1("遮罩极坐标UV1", Vector) = (1,1,0,0)
		[KeywordEnum(R,A)] _MaskChannel1("遮罩通道切换开关1", Float) = 0
		_DisrtortTex_MASK("扭曲贴图_遮罩", 2D) = "white" {}
		_DisrtortUVSpeed3("扭曲UV流动_遮罩", Vector) = (0,0,0,0)
		_DisrtortIntensity3("扭曲强度_遮罩", Float) = 0
		_MaskTex2("遮罩贴图2", 2D) = "white" {}
		[KeywordEnum(R,A)] _MaskChannel2("遮罩通道切换开关2", Float) = 0
		_MaskTexUVSpeed2("遮罩UV流动2", Vector) = (0,0,0,0)
		_DissolveTex("溶解贴图", 2D) = "white" {}
		_DissolveTexUVSpeed("溶解UV流动", Vector) = (0,0,0,0)
		_DissolveTexRotate("溶解旋转", Float) = 0
		[KeywordEnum(Off,On)] _DissolveTexPolar("溶解极坐标开关", Float) = 0
		_DissolveTexPolarUV("溶解极坐标UV", Vector) = (1,1,0,0)
		_DissolveSoft("溶解软硬度", Range( 0 , 1)) = 1
		_DissolveWildth("溶解边宽度", Range( 0 , 1)) = 0
		[HDR]_DissolveWildthColor("溶解边颜色", Color) = (1,1,1,1)
		_DisrtortTex_Dissolve("扭曲贴图_溶解", 2D) = "white" {}
		_DisrtortUVSpeed2("扭曲UV流动_溶解", Vector) = (0,0,0,0)
		_DisrtortIntensity2("扭曲强度_溶解", Float) = 0
		_VextexOffsetTex("顶点偏移贴图", 2D) = "white" {}
		_VextexOffsetIntensity("顶点偏移强度", Float) = 0
		_VextexOffsetTexUVSpeed("顶点偏移UV流动", Vector) = (0,0,0,0)
		_VextexOffsetTexRotate("顶点偏移旋转", Float) = 0
		[KeywordEnum(Off,On)] _VextexOffsetTexPolar("顶点偏移极坐标开关", Float) = 0
		_VextexOffsetTexPolarUV("顶点偏移极坐标UV", Vector) = (1,1,0,0)
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("深度函数", Float) = 0
		[Enum(on,0,off,1)]_Toggle("深度开关", Float) = 0
		_RampTex("RampTex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend [_Src] [_Dst]
		AlphaToMask Off
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite [_Toggle]
		ZTest [_ZTestMode]
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _VEXTEXOFFSETTEXPOLAR_OFF _VEXTEXOFFSETTEXPOLAR_ON
			#pragma shader_feature_local _MAINTEXXULIE_OFF _MAINTEXXULIE_ON
			#pragma shader_feature_local _MAINTEXPOLAR_OFF _MAINTEXPOLAR_ON
			#pragma shader_feature_local _DISRTORTPOLAR1_OFF _DISRTORTPOLAR1_ON
			#pragma shader_feature_local _MASKCHANNEL1_R _MASKCHANNEL1_A
			#pragma shader_feature_local _MASKTEXPOLAR1_OFF _MASKTEXPOLAR1_ON
			#pragma shader_feature_local _DISSOLVETEXPOLAR_OFF _DISSOLVETEXPOLAR_ON
			#pragma shader_feature_local _MASKCHANNEL2_R _MASKCHANNEL2_A


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float3 ase_normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _CullMode;
			uniform float _ZTestMode;
			uniform float _Dst;
			uniform float _Toggle;
			uniform float _Src;
			uniform sampler2D _VextexOffsetTex;
			uniform float2 _VextexOffsetTexUVSpeed;
			uniform float4 _VextexOffsetTex_ST;
			uniform float _VextexOffsetTexRotate;
			uniform float2 _VextexOffsetTexPolarUV;
			uniform float _VextexOffsetIntensity;
			uniform sampler2D _MainTex;
			uniform float2 _MainTexUVSpeed;
			uniform float4 _MainTex_ST;
			uniform float _MainTexRotate;
			uniform float2 _MainTexPolarUV;
			uniform float2 _MainTexXulieKongzhi;
			uniform float _MainTexXulieSpeed;
			uniform float _MainTexXuliePlay;
			uniform sampler2D _DisrtortTex1;
			uniform float2 _DisrtortUVSpeed1;
			uniform float4 _DisrtortTex1_ST;
			uniform float _DisrtortRotate1;
			uniform float2 _DisrtortPolarUV1;
			uniform float _DisrtortIntensity1;
			uniform float4 _MainColor;
			uniform sampler2D _MaskTex1;
			uniform float2 _MaskTexUVSpeed3;
			uniform float4 _MaskTex1_ST;
			uniform float _MaskTexRotate1;
			uniform float2 _MaskTexPolarUV1;
			uniform sampler2D _DisrtortTex_MASK;
			uniform float2 _DisrtortUVSpeed3;
			uniform float4 _DisrtortTex_MASK_ST;
			uniform float _DisrtortIntensity3;
			uniform float _DissolveSoft;
			uniform sampler2D _DissolveTex;
			uniform float2 _DissolveTexUVSpeed;
			uniform float4 _DissolveTex_ST;
			uniform float _DissolveTexRotate;
			uniform float2 _DissolveTexPolarUV;
			uniform sampler2D _DisrtortTex_Dissolve;
			uniform float2 _DisrtortUVSpeed2;
			uniform float4 _DisrtortTex_Dissolve_ST;
			uniform float _DisrtortIntensity2;
			uniform sampler2D _MaskTex2;
			uniform float2 _MaskTexUVSpeed2;
			uniform float4 _MaskTex2_ST;
			uniform sampler2D _RampTex;
			uniform float4 _RampTex_ST;
			uniform float _DissolveWildth;
			uniform float4 _DissolveWildthColor;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float2 uv_VextexOffsetTex = v.ase_texcoord * _VextexOffsetTex_ST.xy + _VextexOffsetTex_ST.zw;
				float cos109 = cos( ( (-1.0 + (_VextexOffsetTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float sin109 = sin( ( (-1.0 + (_VextexOffsetTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float2 rotator109 = mul( uv_VextexOffsetTex - float2( 0.5,0.5 ) , float2x2( cos109 , -sin109 , sin109 , cos109 )) + float2( 0.5,0.5 );
				float2 CenteredUV15_g15 = ( rotator109 - float2( 0.5,0.5 ) );
				float2 break17_g15 = CenteredUV15_g15;
				float2 appendResult23_g15 = (float2(( length( CenteredUV15_g15 ) * _VextexOffsetTexPolarUV.x * 2.0 ) , ( atan2( break17_g15.x , break17_g15.y ) * ( 1.0 / 6.28318548202515 ) * _VextexOffsetTexPolarUV.y )));
				#if defined(_VEXTEXOFFSETTEXPOLAR_OFF)
				float2 staticSwitch112 = rotator109;
				#elif defined(_VEXTEXOFFSETTEXPOLAR_ON)
				float2 staticSwitch112 = appendResult23_g15;
				#else
				float2 staticSwitch112 = rotator109;
				#endif
				float2 panner113 = ( 1.0 * _Time.y * _VextexOffsetTexUVSpeed + staticSwitch112);
				float3 desaturateInitialColor115 = tex2Dlod( _VextexOffsetTex, float4( panner113, 0, 0.0) ).rgb;
				float desaturateDot115 = dot( desaturateInitialColor115, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar115 = lerp( desaturateInitialColor115, desaturateDot115.xxx, 1.0 );
				float VextexOffset117 = (desaturateVar115).x;
				
				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_color = v.color;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = ( VextexOffset117 * v.ase_normal * _VextexOffsetIntensity );
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float cos5 = cos( ( (-1.0 + (_MainTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float sin5 = sin( ( (-1.0 + (_MainTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float2 rotator5 = mul( uv_MainTex - float2( 0.5,0.5 ) , float2x2( cos5 , -sin5 , sin5 , cos5 )) + float2( 0.5,0.5 );
				float2 CenteredUV15_g13 = ( rotator5 - float2( 0.5,0.5 ) );
				float2 break17_g13 = CenteredUV15_g13;
				float2 appendResult23_g13 = (float2(( length( CenteredUV15_g13 ) * _MainTexPolarUV.x * 2.0 ) , ( atan2( break17_g13.x , break17_g13.y ) * ( 1.0 / 6.28318548202515 ) * _MainTexPolarUV.y )));
				#if defined(_MAINTEXPOLAR_OFF)
				float2 staticSwitch9 = rotator5;
				#elif defined(_MAINTEXPOLAR_ON)
				float2 staticSwitch9 = appendResult23_g13;
				#else
				float2 staticSwitch9 = rotator5;
				#endif
				float2 panner3 = ( 1.0 * _Time.y * _MainTexUVSpeed + staticSwitch9);
				float temp_output_4_0_g14 = _MainTexXulieKongzhi.x;
				float temp_output_5_0_g14 = _MainTexXulieKongzhi.y;
				float2 appendResult7_g14 = (float2(temp_output_4_0_g14 , temp_output_5_0_g14));
				float totalFrames39_g14 = ( temp_output_4_0_g14 * temp_output_5_0_g14 );
				float2 appendResult8_g14 = (float2(totalFrames39_g14 , temp_output_5_0_g14));
				float lerpResult22 = lerp( 0.0 , ( ( _MainTexXulieKongzhi.x * _MainTexXulieKongzhi.y ) - 0.0 ) , _MainTexXuliePlay);
				float clampResult42_g14 = clamp( lerpResult22 , 0.0001 , ( totalFrames39_g14 - 1.0 ) );
				float temp_output_35_0_g14 = frac( ( ( ( _Time.y * _MainTexXulieSpeed ) + clampResult42_g14 ) / totalFrames39_g14 ) );
				float2 appendResult29_g14 = (float2(temp_output_35_0_g14 , ( 1.0 - temp_output_35_0_g14 )));
				float2 temp_output_15_0_g14 = ( ( uv_MainTex / appendResult7_g14 ) + ( floor( ( appendResult8_g14 * appendResult29_g14 ) ) / appendResult7_g14 ) );
				#if defined(_MAINTEXXULIE_OFF)
				float2 staticSwitch19 = panner3;
				#elif defined(_MAINTEXXULIE_ON)
				float2 staticSwitch19 = temp_output_15_0_g14;
				#else
				float2 staticSwitch19 = panner3;
				#endif
				float2 uv_DisrtortTex1 = i.ase_texcoord1.xy * _DisrtortTex1_ST.xy + _DisrtortTex1_ST.zw;
				float cos32 = cos( ( (-1.0 + (_DisrtortRotate1 - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float sin32 = sin( ( (-1.0 + (_DisrtortRotate1 - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float2 rotator32 = mul( uv_DisrtortTex1 - float2( 0.5,0.5 ) , float2x2( cos32 , -sin32 , sin32 , cos32 )) + float2( 0.5,0.5 );
				float2 CenteredUV15_g3 = ( rotator32 - float2( 0.5,0.5 ) );
				float2 break17_g3 = CenteredUV15_g3;
				float2 appendResult23_g3 = (float2(( length( CenteredUV15_g3 ) * _DisrtortPolarUV1.x * 2.0 ) , ( atan2( break17_g3.x , break17_g3.y ) * ( 1.0 / 6.28318548202515 ) * _DisrtortPolarUV1.y )));
				#if defined(_DISRTORTPOLAR1_OFF)
				float2 staticSwitch34 = rotator32;
				#elif defined(_DISRTORTPOLAR1_ON)
				float2 staticSwitch34 = appendResult23_g3;
				#else
				float2 staticSwitch34 = rotator32;
				#endif
				float2 panner36 = ( 1.0 * _Time.y * _DisrtortUVSpeed1 + staticSwitch34);
				float3 desaturateInitialColor38 = tex2D( _DisrtortTex1, panner36 ).rgb;
				float desaturateDot38 = dot( desaturateInitialColor38, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar38 = lerp( desaturateInitialColor38, desaturateDot38.xxx, 1.0 );
				float2 Disrtort44 = ( (desaturateVar38).xy * _DisrtortIntensity1 );
				float4 MainTex46 = ( tex2D( _MainTex, ( staticSwitch19 + Disrtort44 ) ) * _MainColor );
				float2 uv_MaskTex1 = i.ase_texcoord1.xy * _MaskTex1_ST.xy + _MaskTex1_ST.zw;
				float cos54 = cos( ( (-1.0 + (_MaskTexRotate1 - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float sin54 = sin( ( (-1.0 + (_MaskTexRotate1 - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float2 rotator54 = mul( uv_MaskTex1 - float2( 0.5,0.5 ) , float2x2( cos54 , -sin54 , sin54 , cos54 )) + float2( 0.5,0.5 );
				float2 CenteredUV15_g12 = ( rotator54 - float2( 0.5,0.5 ) );
				float2 break17_g12 = CenteredUV15_g12;
				float2 appendResult23_g12 = (float2(( length( CenteredUV15_g12 ) * _MaskTexPolarUV1.x * 2.0 ) , ( atan2( break17_g12.x , break17_g12.y ) * ( 1.0 / 6.28318548202515 ) * _MaskTexPolarUV1.y )));
				#if defined(_MASKTEXPOLAR1_OFF)
				float2 staticSwitch56 = rotator54;
				#elif defined(_MASKTEXPOLAR1_ON)
				float2 staticSwitch56 = appendResult23_g12;
				#else
				float2 staticSwitch56 = rotator54;
				#endif
				float2 panner58 = ( 1.0 * _Time.y * _MaskTexUVSpeed3 + staticSwitch56);
				float2 uv_DisrtortTex_MASK = i.ase_texcoord1.xy * _DisrtortTex_MASK_ST.xy + _DisrtortTex_MASK_ST.zw;
				float2 panner174 = ( 1.0 * _Time.y * _DisrtortUVSpeed3 + uv_DisrtortTex_MASK);
				float4 lerpResult177 = lerp( float4( panner58, 0.0 , 0.0 ) , tex2D( _DisrtortTex_MASK, panner174 ) , _DisrtortIntensity3);
				float4 tex2DNode59 = tex2D( _MaskTex1, lerpResult177.rg );
				float3 desaturateInitialColor60 = tex2DNode59.rgb;
				float desaturateDot60 = dot( desaturateInitialColor60, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar60 = lerp( desaturateInitialColor60, desaturateDot60.xxx, 1.0 );
				#if defined(_MASKCHANNEL1_R)
				float staticSwitch67 = (desaturateVar60).x;
				#elif defined(_MASKCHANNEL1_A)
				float staticSwitch67 = tex2DNode59.a;
				#else
				float staticSwitch67 = (desaturateVar60).x;
				#endif
				float Mask164 = staticSwitch67;
				float temp_output_89_0 = ( 1.0 - _DissolveSoft );
				float2 uv_DissolveTex = i.ase_texcoord1.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float cos74 = cos( ( (-1.0 + (_DissolveTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float sin74 = sin( ( (-1.0 + (_DissolveTexRotate - -360.0) * (1.0 - -1.0) / (360.0 - -360.0)) * 6.28318548202515 ) );
				float2 rotator74 = mul( uv_DissolveTex - float2( 0.5,0.5 ) , float2x2( cos74 , -sin74 , sin74 , cos74 )) + float2( 0.5,0.5 );
				float2 CenteredUV15_g5 = ( rotator74 - float2( 0.5,0.5 ) );
				float2 break17_g5 = CenteredUV15_g5;
				float2 appendResult23_g5 = (float2(( length( CenteredUV15_g5 ) * _DissolveTexPolarUV.x * 2.0 ) , ( atan2( break17_g5.x , break17_g5.y ) * ( 1.0 / 6.28318548202515 ) * _DissolveTexPolarUV.y )));
				#if defined(_DISSOLVETEXPOLAR_OFF)
				float2 staticSwitch77 = rotator74;
				#elif defined(_DISSOLVETEXPOLAR_ON)
				float2 staticSwitch77 = appendResult23_g5;
				#else
				float2 staticSwitch77 = rotator74;
				#endif
				float2 panner79 = ( 1.0 * _Time.y * _DissolveTexUVSpeed + staticSwitch77);
				float2 uv_DisrtortTex_Dissolve = i.ase_texcoord1.xy * _DisrtortTex_Dissolve_ST.xy + _DisrtortTex_Dissolve_ST.zw;
				float2 panner163 = ( 1.0 * _Time.y * _DisrtortUVSpeed2 + uv_DisrtortTex_Dissolve);
				float4 lerpResult169 = lerp( float4( panner79, 0.0 , 0.0 ) , tex2D( _DisrtortTex_Dissolve, panner163 ) , _DisrtortIntensity2);
				float3 desaturateInitialColor81 = tex2D( _DissolveTex, lerpResult169.rg ).rgb;
				float desaturateDot81 = dot( desaturateInitialColor81, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar81 = lerp( desaturateInitialColor81, desaturateDot81.xxx, 1.0 );
				float4 texCoord171 = i.ase_texcoord1;
				texCoord171.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult87 = clamp( ( (desaturateVar81).x + ( texCoord171.z * -2.0 ) ) , 0.0 , 1.0 );
				float smoothstepResult88 = smoothstep( 0.0 , temp_output_89_0 , clampResult87);
				float Dissolve99 = smoothstepResult88;
				float2 uv_MaskTex2 = i.ase_texcoord1.xy * _MaskTex2_ST.xy + _MaskTex2_ST.zw;
				float2 panner145 = ( 1.0 * _Time.y * _MaskTexUVSpeed2 + uv_MaskTex2);
				float4 tex2DNode146 = tex2D( _MaskTex2, panner145 );
				float3 desaturateInitialColor147 = tex2DNode146.rgb;
				float desaturateDot147 = dot( desaturateInitialColor147, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar147 = lerp( desaturateInitialColor147, desaturateDot147.xxx, 1.0 );
				#if defined(_MASKCHANNEL2_R)
				float staticSwitch149 = (desaturateVar147).x;
				#elif defined(_MASKCHANNEL2_A)
				float staticSwitch149 = tex2DNode146.a;
				#else
				float staticSwitch149 = (desaturateVar147).x;
				#endif
				float Mask2150 = staticSwitch149;
				float2 uv_RampTex = i.ase_texcoord1.xy * _RampTex_ST.xy + _RampTex_ST.zw;
				float smoothstepResult94 = smoothstep( 0.0 , ( temp_output_89_0 + _DissolveWildth ) , clampResult87);
				float DissolveWildth96 = ( smoothstepResult88 - smoothstepResult94 );
				float4 temp_output_98_0 = ( ( MainTex46 * Mask164 * Dissolve99 * i.ase_color * Mask2150 * tex2D( _RampTex, uv_RampTex ) ) + ( DissolveWildth96 * _DissolveWildthColor ) );
				float4 appendResult15 = (float4((temp_output_98_0).rgb , (temp_output_98_0).a));
				
				
				finalColor = appendResult15;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
2068;61.6;1910.4;998.2;2090.4;-219.5379;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;43;-2271.921,909.0599;Inherit;False;2611.767;527.6378;Comment;16;26;27;28;30;29;32;31;33;34;35;36;37;38;39;42;41;扭曲;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2224.059,1145.925;Inherit;False;Property;_DisrtortRotate1;扭曲旋转1;13;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;122;-2271.062,3198.221;Inherit;False;3653.158;834.3035;Comment;32;95;94;88;87;92;91;84;89;90;85;82;86;81;80;169;164;79;167;77;78;163;76;161;157;75;74;73;72;71;70;69;171;溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.TauNode;28;-2027.005,1326.298;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;27;-2035.733,1138.652;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-360;False;2;FLOAT;360;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2259.051,3429.447;Inherit;False;Property;_DissolveTexRotate;溶解旋转;30;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1762.264,1141.562;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;30;-1930.987,959.0599;Inherit;False;0;37;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;71;-2075.918,3436.583;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-360;False;2;FLOAT;360;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TauNode;70;-2059.918,3612.583;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;32;-1593.53,1022.129;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;31;-1611.234,1207.976;Inherit;False;Property;_DisrtortPolarUV1;扭曲极坐标UV1;15;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;73;-1963.918,3244.583;Inherit;False;0;80;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1787.918,3436.583;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;33;-1339.9,1090.81;Inherit;False;Polar Coordinates;-1;;3;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;74;-1627.918,3308.583;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;75;-1643.918,3500.583;Inherit;False;Property;_DissolveTexPolarUV;溶解极坐标UV;32;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;24;-2253.928,-156.3228;Inherit;False;2615.366;942.969;Comment;19;125;1;40;19;45;3;9;4;8;12;5;2;11;25;10;7;6;126;133;主帖图;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-2573,1823.934;Inherit;False;Property;_MaskTexRotate1;遮罩旋转1;18;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;68;-2258.361,1622.378;Inherit;False;2679.369;789.2228;Comment;19;176;174;173;172;175;67;61;60;59;58;56;57;55;53;54;51;52;50;177;遮罩1;1,1,1,1;0;0
Node;AmplifyShaderEditor.TFHCRemapNode;50;-2414.549,1831.119;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-360;False;2;FLOAT;360;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TauNode;49;-2455.104,2033.929;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-2203.928,78.98264;Inherit;False;Property;_MainTexRotate;主帖图旋转;3;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;35;-1165.655,1271.343;Inherit;False;Property;_DisrtortUVSpeed1;扭曲UV流动1;12;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.StaticSwitch;34;-1084.571,1023.738;Inherit;False;Property;_DisrtortPolar1;扭曲极坐标开关1;14;0;Create;False;0;0;0;False;0;False;0;0;0;True;ON;KeywordEnum;2;Off;On;Create;True;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;161;-1593.142,3871.997;Inherit;False;Property;_DisrtortUVSpeed2;扭曲UV流动_溶解;38;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;76;-1371.918,3388.583;Inherit;False;Polar Coordinates;-1;;5;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;157;-1614.126,3738.768;Inherit;False;0;164;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;163;-1351.908,3756.498;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;36;-824.7443,1096.265;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TauNode;10;-2009.013,259.3548;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2190.364,1849.193;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;25;-1811.152,318.7483;Inherit;False;1108.814;485.8312;Comment;9;20;17;16;22;18;23;21;178;179;读取序列;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;78;-1195.918,3564.583;Inherit;False;Property;_DissolveTexUVSpeed;溶解UV流动;29;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TFHCRemapNode;7;-2017.741,71.70955;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-360;False;2;FLOAT;360;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;52;-2264.199,1692.796;Inherit;False;0;59;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;77;-1115.918,3308.583;Inherit;False;Property;_DissolveTexPolar;溶解极坐标开关;31;0;Create;False;0;0;0;False;0;False;0;0;0;True;ON;KeywordEnum;2;Off;On;Create;True;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;37;-564.8988,1073.657;Inherit;True;Property;_DisrtortTex1;扭曲贴图1;10;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;167;-991.1683,3946.165;Inherit;False;Property;_DisrtortIntensity2;扭曲强度_溶解;39;0;Create;False;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;17;-1744.968,368.7483;Inherit;False;Property;_MainTexXulieKongzhi;主帖图序列排列;7;0;Create;False;0;0;0;False;0;False;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1865.135,-67.80006;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;53;-2039.334,1916.676;Inherit;False;Property;_MaskTexPolarUV1;遮罩极坐标UV1;20;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;79;-859.9182,3388.583;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;164;-1149.497,3732.172;Inherit;True;Property;_DisrtortTex_Dissolve;扭曲贴图_溶解;37;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;54;-1991.209,1698.225;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-1744.272,74.6188;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;123;-2294.784,4218.439;Inherit;False;2417.997;532.2;Comment;14;103;105;104;107;106;108;109;110;112;111;113;114;115;116;顶点偏移;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;5;-1557.136,-62.03172;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-1476.569,508.6106;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;172;-1705.988,2272.717;Inherit;False;Property;_DisrtortUVSpeed3;扭曲UV流动_遮罩;23;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DesaturateOpNode;38;-246.1982,1081.262;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-1729.813,2138.488;Inherit;False;0;176;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;133;-1554.885,64.86053;Inherit;False;Constant;_Vector0;Vector 0;41;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;169;-648.4059,3387.391;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;55;-1768,1798.441;Inherit;False;Polar Coordinates;-1;;12;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;12;-1562.606,195.7153;Inherit;False;Property;_MainTexPolarUV;主帖图极坐标UV;5;0;Create;False;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;174;-1465.754,2156.218;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-2244.784,4460.44;Inherit;False;Property;_VextexOffsetTexRotate;顶点偏移旋转;43;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;80;-469.6922,3360.221;Inherit;True;Property;_DissolveTex;溶解贴图;28;0;Create;False;0;0;0;False;0;False;-1;None;e0e88fc8aa5560c4dbbe90ba0eb1d012;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-1334.814,733.5036;Inherit;False;Property;_MainTexXulieSpeed;主帖图序列速度;6;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;57;-1593.755,1978.974;Inherit;False;Property;_MaskTexUVSpeed3;遮罩UV流动1;17;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;23;-1325.997,517.6448;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-1720.152,592.0214;Inherit;False;Property;_MainTexXuliePlay;主帖图序列滑杆;9;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;134;-2247.162,2609.459;Inherit;False;2656.622;527.6379;Comment;7;149;148;147;146;145;144;139;遮罩2;1,1,1,1;0;0
Node;AmplifyShaderEditor.StaticSwitch;56;-1461.289,1766.547;Inherit;False;Property;_MaskTexPolar1;遮罩极坐标开关1;19;0;Create;False;0;0;0;False;0;False;0;0;0;True;ON;KeywordEnum;2;Off;On;Create;True;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;178;-1338.353,654.7844;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;39;-70.58877,1089.607;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-90.46544,1187.226;Inherit;False;Property;_DisrtortIntensity1;扭曲强度1;11;0;Create;False;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;8;-1321.908,23.86766;Inherit;False;Polar Coordinates;-1;;13;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TauNode;105;-2052.79,4636.44;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;139;-1161.142,2777.612;Inherit;False;0;146;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;104;-2068.79,4460.44;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-360;False;2;FLOAT;360;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;176;-1264.939,2134.299;Inherit;True;Property;_DisrtortTex_MASK;扭曲贴图_遮罩;22;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;171;-299.1776,3697.306;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;9;-1065.019,-43.20497;Inherit;False;Property;_MainTexPolar;主帖图极坐标开关;4;0;Create;False;0;0;0;False;0;False;0;0;0;True;ON;KeywordEnum;2;Off;On;Create;True;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;153.9088,1120.794;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;4;-1147.663,204.4002;Inherit;False;Property;_MainTexUVSpeed;主贴图UV流动;2;0;Create;False;0;0;0;False;0;False;0,0;0.2,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;144;-1121.396,2940.541;Inherit;False;Property;_MaskTexUVSpeed2;遮罩UV流动2;27;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DesaturateOpNode;81;-165.6917,3376.221;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;58;-1189.324,1788.089;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;22;-1175.918,513.2306;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;175;-1124.132,2347.56;Inherit;False;Property;_DisrtortIntensity3;扭曲强度_遮罩;24;0;Create;False;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;179;-1088.353,690.7844;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;3;-806.7523,29.32262;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;434.3838,1125.836;Inherit;False;Disrtort;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;107;-1956.79,4268.44;Inherit;False;0;114;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;177;-790.126,1806.941;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;82;10.30852,3392.221;Inherit;False;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;106.3085,3584.221;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-1780.79,4460.44;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;261.0807,3597.729;Inherit;False;Property;_DissolveSoft;溶解软硬度;34;0;Create;False;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;16;-994.7376,380.6237;Inherit;False;Flipbook;-1;;14;53c2488c220f6564ca6c90721ee16673;2,71,0,68,0;8;51;SAMPLER2D;0.0;False;13;FLOAT2;0,0;False;4;FLOAT;3;False;5;FLOAT;3;False;24;FLOAT;0;False;2;FLOAT;0;False;55;FLOAT;0;False;70;FLOAT;0;False;5;COLOR;53;FLOAT2;0;FLOAT;47;FLOAT;48;FLOAT;62
Node;AmplifyShaderEditor.PannerNode;145;-805.1855,2792.763;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;91;274.1273,3741.242;Inherit;False;Property;_DissolveWildth;溶解边宽度;35;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;59;-523.7896,1780.661;Inherit;True;Property;_MaskTex1;遮罩贴图1;16;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;146;-549.6376,2767.953;Inherit;True;Property;_MaskTex2;遮罩贴图2;25;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;84;263.3629,3377.209;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;108;-1636.79,4524.44;Inherit;False;Property;_VextexOffsetTexPolarUV;顶点偏移极坐标UV;45;0;Create;False;0;0;0;False;0;False;1,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RotatorNode;109;-1620.79,4332.44;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;19;-535.4919,45.63783;Inherit;False;Property;_MainTexXulie;主帖图序列开关;8;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;Off;On;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;89;549.0807,3600.729;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;45;-525.4843,258.199;Inherit;False;44;Disrtort;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode;60;-215.7749,1794.58;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;702.4957,3740.514;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;87;396.0808,3382.729;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-295.4828,83.62818;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;110;-1364.79,4412.44;Inherit;False;Polar Coordinates;-1;;15;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode;147;-227.323,2781.66;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;94;919.3834,3665.998;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;111;-1188.79,4588.439;Inherit;False;Property;_VextexOffsetTexUVSpeed;顶点偏移UV流动;42;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ComponentMaskNode;148;-45.83012,2790.005;Inherit;False;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;61;-34.28193,1801.413;Inherit;False;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;125;-51.39001,217.2003;Inherit;False;Property;_MainColor;主帖图颜色;1;1;[HDR];Create;False;0;0;0;False;0;False;0,0,0,0;2,2,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-148.6301,21.59886;Inherit;True;Property;_MainTex;主帖图;0;0;Create;False;0;0;0;False;0;False;-1;None;126cfdd22cddc784c86452dd79bad526;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;112;-1110.09,4329.839;Inherit;False;Property;_VextexOffsetTexPolar;顶点偏移极坐标开关;44;0;Create;False;0;0;0;False;0;False;0;0;0;True;ON;KeywordEnum;2;Off;On;Create;True;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;88;708.0807,3378.729;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;113;-852.79,4412.44;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;184.0397,48.99238;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;149;173.46,2787.501;Inherit;False;Property;_MaskChannel2;遮罩通道切换开关2;26;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;R;A;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;67;185.0082,1802.248;Inherit;False;Property;_MaskChannel1;遮罩通道切换开关1;21;0;Create;False;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;2;R;A;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;95;1216.496,3508.514;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;114;-611.9897,4390.839;Inherit;True;Property;_VextexOffsetTex;顶点偏移贴图;40;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;1531.562,3377.466;Inherit;False;Dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;492.0352,1795.219;Inherit;False;Mask1;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;422.8403,-49.89738;Inherit;False;MainTex;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;96;1543.49,3505.711;Inherit;False;DissolveWildth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;492.0462,2788.639;Inherit;False;Mask2;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;124;561.3947,-185.4437;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;151;592.8653,313.7335;Inherit;False;150;Mask2;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;115;-276.7897,4396.44;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;585.7507,124.1877;Inherit;False;64;Mask1;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;47;584.2063,23.89664;Inherit;False;46;MainTex;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;97;588.4308,403.333;Inherit;False;96;DissolveWildth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;592.0052,224.0223;Inherit;False;99;Dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;101;593.5402,498.3126;Inherit;False;Property;_DissolveWildthColor;溶解边颜色;36;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;170;457.2002,-401.0983;Inherit;True;Property;_RampTex;RampTex;51;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;116;-100.7893,4412.44;Inherit;False;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;102;877.916,472.76;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;809.5476,63.08691;Inherit;False;6;6;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;5;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;229.6429,4421.981;Inherit;False;VextexOffset;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;1003.566,55.21567;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;119;1225.964,353.1428;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;121;1235.964,503.1428;Inherit;False;Property;_VextexOffsetIntensity;顶点偏移强度;41;0;Create;False;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;118;1210.964,268.1429;Inherit;False;117;VextexOffset;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;14;1196.779,137.5853;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;13;1196.063,15.06493;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-4171.726,2047.74;Inherit;False;Property;_Dst;Dst;48;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-4170.726,1963.74;Inherit;False;Property;_Src;Src;47;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-356.3326,3575.613;Inherit;False;Property;_DissolveIntensity;溶解进程;33;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;1513.412,54.00351;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-4169.726,2135.74;Inherit;False;Property;_ZTestMode;深度函数;49;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-4169.726,2222.74;Inherit;False;Property;_Toggle;深度开关;50;1;[Enum];Create;False;0;2;on;0;off;1;0;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;127;-4171.086,1878.486;Inherit;False;Property;_CullMode;剔除模式;46;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;1455.964,347.1428;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1716.544,50.08413;Float;False;True;-1;2;ASEMaterialInspector;100;1;tongyong;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;1;5;True;128;10;True;129;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;True;127;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;True;131;True;3;True;130;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;27;0;26;0
WireConnection;29;0;27;0
WireConnection;29;1;28;0
WireConnection;71;0;69;0
WireConnection;32;0;30;0
WireConnection;32;2;29;0
WireConnection;72;0;71;0
WireConnection;72;1;70;0
WireConnection;33;1;32;0
WireConnection;33;3;31;1
WireConnection;33;4;31;2
WireConnection;74;0;73;0
WireConnection;74;2;72;0
WireConnection;50;0;48;0
WireConnection;34;1;32;0
WireConnection;34;0;33;0
WireConnection;76;1;74;0
WireConnection;76;3;75;1
WireConnection;76;4;75;2
WireConnection;163;0;157;0
WireConnection;163;2;161;0
WireConnection;36;0;34;0
WireConnection;36;2;35;0
WireConnection;51;0;50;0
WireConnection;51;1;49;0
WireConnection;7;0;6;0
WireConnection;77;1;74;0
WireConnection;77;0;76;0
WireConnection;37;1;36;0
WireConnection;79;0;77;0
WireConnection;79;2;78;0
WireConnection;164;1;163;0
WireConnection;54;0;52;0
WireConnection;54;2;51;0
WireConnection;11;0;7;0
WireConnection;11;1;10;0
WireConnection;5;0;2;0
WireConnection;5;2;11;0
WireConnection;21;0;17;1
WireConnection;21;1;17;2
WireConnection;38;0;37;0
WireConnection;169;0;79;0
WireConnection;169;1;164;0
WireConnection;169;2;167;0
WireConnection;55;1;54;0
WireConnection;55;3;53;1
WireConnection;55;4;53;2
WireConnection;174;0;173;0
WireConnection;174;2;172;0
WireConnection;80;1;169;0
WireConnection;23;0;21;0
WireConnection;56;1;54;0
WireConnection;56;0;55;0
WireConnection;39;0;38;0
WireConnection;8;1;5;0
WireConnection;8;2;133;0
WireConnection;8;3;12;1
WireConnection;8;4;12;2
WireConnection;104;0;103;0
WireConnection;176;1;174;0
WireConnection;9;1;5;0
WireConnection;9;0;8;0
WireConnection;41;0;39;0
WireConnection;41;1;42;0
WireConnection;81;0;80;0
WireConnection;58;0;56;0
WireConnection;58;2;57;0
WireConnection;22;1;23;0
WireConnection;22;2;20;0
WireConnection;179;0;178;0
WireConnection;179;1;18;0
WireConnection;3;0;9;0
WireConnection;3;2;4;0
WireConnection;44;0;41;0
WireConnection;177;0;58;0
WireConnection;177;1;176;0
WireConnection;177;2;175;0
WireConnection;82;0;81;0
WireConnection;85;0;171;3
WireConnection;106;0;104;0
WireConnection;106;1;105;0
WireConnection;16;13;2;0
WireConnection;16;4;17;1
WireConnection;16;5;17;2
WireConnection;16;24;22;0
WireConnection;16;2;179;0
WireConnection;145;0;139;0
WireConnection;145;2;144;0
WireConnection;59;1;177;0
WireConnection;146;1;145;0
WireConnection;84;0;82;0
WireConnection;84;1;85;0
WireConnection;109;0;107;0
WireConnection;109;2;106;0
WireConnection;19;1;3;0
WireConnection;19;0;16;0
WireConnection;89;0;90;0
WireConnection;60;0;59;0
WireConnection;92;0;89;0
WireConnection;92;1;91;0
WireConnection;87;0;84;0
WireConnection;40;0;19;0
WireConnection;40;1;45;0
WireConnection;110;1;109;0
WireConnection;110;3;108;1
WireConnection;110;4;108;2
WireConnection;147;0;146;0
WireConnection;94;0;87;0
WireConnection;94;2;92;0
WireConnection;148;0;147;0
WireConnection;61;0;60;0
WireConnection;1;1;40;0
WireConnection;112;1;109;0
WireConnection;112;0;110;0
WireConnection;88;0;87;0
WireConnection;88;2;89;0
WireConnection;113;0;112;0
WireConnection;113;2;111;0
WireConnection;126;0;1;0
WireConnection;126;1;125;0
WireConnection;149;1;148;0
WireConnection;149;0;146;4
WireConnection;67;1;61;0
WireConnection;67;0;59;4
WireConnection;95;0;88;0
WireConnection;95;1;94;0
WireConnection;114;1;113;0
WireConnection;99;0;88;0
WireConnection;64;0;67;0
WireConnection;46;0;126;0
WireConnection;96;0;95;0
WireConnection;150;0;149;0
WireConnection;115;0;114;0
WireConnection;116;0;115;0
WireConnection;102;0;97;0
WireConnection;102;1;101;0
WireConnection;66;0;47;0
WireConnection;66;1;65;0
WireConnection;66;2;100;0
WireConnection;66;3;124;0
WireConnection;66;4;151;0
WireConnection;66;5;170;0
WireConnection;117;0;116;0
WireConnection;98;0;66;0
WireConnection;98;1;102;0
WireConnection;14;0;98;0
WireConnection;13;0;98;0
WireConnection;15;0;13;0
WireConnection;15;3;14;0
WireConnection;120;0;118;0
WireConnection;120;1;119;0
WireConnection;120;2;121;0
WireConnection;0;0;15;0
WireConnection;0;1;120;0
ASEEND*/
//CHKSM=260992604CA298D8B1CCA5821B68CFFE1506AFF2