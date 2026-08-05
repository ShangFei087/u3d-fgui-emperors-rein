// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hologram"
{
	Properties
	{
		[HDR]_BaseColor("BaseColor", Color) = (1,1,1,1)
		_Tiling("Tiling", Float) = 200
		[Toggle]_ZWriteMode("ZWriteMode", Float) = 0
		_NormalMap("NormalMap", 2D) = "bump" {}
		_RimBias("RimBias", Float) = 0
		_RimPower("RimPower", Float) = 2
		_RimScale("RimScale", Float) = 1
		_WireFrame("WireFrame", 2D) = "white" {}
		_WireFrameIntensity("WireFrameIntensity", Float) = 1
		_FlickCintrol("FlickCintrol", Range( 0 , 1)) = 0
		_Alpha("Alpha", Range( 0 , 1)) = 0
		[HDR]_ScanLineColor("ScanLineColor", Color) = (1,1,1,1)
		_ScanLineTex("ScanLineTex", 2D) = "white" {}
		_ScanLineTiling("ScanLineTiling", Float) = 0
		_ScanLineSpeed("ScanLineSpeed", Float) = 0
		_ScanLineInvert("ScanLineInvert", Float) = 0
		_ScanLinePower("ScanLinePower", Float) = 0
		_ScanLineAlpha("ScanLineAlpha", Float) = 1
		_RandomVertexOffset("RandomVertexOffset", Vector) = (0,0,0,0)
		_RandomGlicthTiling("RandomGlicthTiling", Float) = 5
		_ScanLineGlitchTex("ScanLineGlitchTex", 2D) = "white" {}
		_ScanLineGlitchOffset("ScanLineGlitchOffset", Vector) = (0,0,0,0)
		_ScanLineGlichTiling("ScanLineGlichTiling", Float) = 0
		_ScanLineGlichSpeed("ScanLineGlichSpeed", Float) = 0
		_ScanLineGlichInvert("ScanLineGlichInvert", Float) = 0
		_ScanLineGlichPower("ScanLineGlichPower", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite [_ZWriteMode]
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float vertexToFrag135;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform float _ZWriteMode;
		uniform float3 _RandomVertexOffset;
		uniform float _RandomGlicthTiling;
		uniform float _Tiling;
		uniform float3 _ScanLineGlitchOffset;
		uniform sampler2D _ScanLineGlitchTex;
		uniform float _ScanLineGlichTiling;
		uniform float _ScanLineGlichSpeed;
		uniform float _ScanLineGlichInvert;
		uniform float _ScanLineGlichPower;
		uniform float _FlickCintrol;
		uniform float4 _BaseColor;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _RimBias;
		uniform float _RimScale;
		uniform float _RimPower;
		uniform sampler2D _ScanLineTex;
		uniform float _ScanLineTiling;
		uniform float _ScanLineSpeed;
		uniform float _ScanLineInvert;
		uniform float _ScanLinePower;
		uniform float4 _ScanLineColor;
		uniform float _ScanLineAlpha;
		uniform sampler2D _WireFrame;
		uniform float4 _WireFrame_ST;
		uniform float _WireFrameIntensity;
		uniform float _Alpha;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 viewToObjDir83 = mul( UNITY_MATRIX_T_MV, float4( _RandomVertexOffset, 0 ) ).xyz;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float mulTime72 = _Time.y * -2.5;
			float mulTime74 = _Time.y * -2.0;
			float2 appendResult73 = (float2((ase_worldPos.y*_RandomGlicthTiling + mulTime72) , mulTime74));
			float simplePerlin2D75 = snoise( appendResult73 );
			simplePerlin2D75 = simplePerlin2D75*0.5 + 0.5;
			float3 objToWorld7 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float mulTime6 = _Time.y * 10.0;
			float mulTime13 = _Time.y * 0.5;
			float2 appendResult12 = (float2((( objToWorld7.x + objToWorld7.y + objToWorld7.z )*_Tiling + mulTime6) , mulTime13));
			float simplePerlin3D9 = snoise( float3( appendResult12 ,  0.0 ) );
			simplePerlin3D9 = simplePerlin3D9*0.5 + 0.5;
			float flicker_vertex92 = simplePerlin3D9;
			float clampResult95 = clamp( (flicker_vertex92*2.0 + -1.0) , 0.0 , 1.0 );
			float temp_output_96_0 = ( (simplePerlin2D75*2.0 + -1.0) * clampResult95 );
			float simplePerlin2D101 = snoise( ( appendResult73 * 20.0 ) );
			simplePerlin2D101 = simplePerlin2D101*0.5 + 0.5;
			float clampResult103 = clamp( (simplePerlin2D101*2.0 + -1.0) , 0.0 , 1.0 );
			float3 RandomGlicth81 = ( ( viewToObjDir83 * 0.01 ) * ( temp_output_96_0 + ( temp_output_96_0 * clampResult103 ) ) );
			float3 viewToObjDir127 = mul( UNITY_MATRIX_T_MV, float4( _ScanLineGlitchOffset, 0 ) ).xyz;
			float3 objToWorld110 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float mulTime114 = _Time.y * _ScanLineGlichSpeed;
			float2 appendResult117 = (float2(0.5 , ( ( ( ase_worldPos.y - objToWorld110.y ) * _ScanLineGlichTiling ) + mulTime114 )));
			float clampResult134 = clamp( ( ( tex2Dlod( _ScanLineGlitchTex, float4( appendResult117, 0, 0.0) ).r - _ScanLineGlichInvert ) * _ScanLineGlichPower ) , 0.0 , 1.0 );
			float3 ScanLineGlitch131 = ( ( viewToObjDir127 * 0.01 ) * clampResult134 );
			v.vertex.xyz += ( RandomGlicth81 + ScanLineGlitch131 );
			v.vertex.w = 1;
			float clampResult16 = clamp( (-0.5 + (simplePerlin3D9 - 0.0) * (2.0 - -0.5) / (1.0 - 0.0)) , 0.0 , 1.0 );
			float lerpResult41 = lerp( 1.0 , clampResult16 , _FlickCintrol);
			o.vertexToFrag135 = lerpResult41;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float Flicking17 = i.vertexToFrag135;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float fresnelNdotV20 = dot( normalize( (WorldNormalVector( i , UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) ) )) ), ase_worldViewDir );
			float fresnelNode20 = ( _RimBias + _RimScale * pow( max( 1.0 - fresnelNdotV20 , 0.0001 ), _RimPower ) );
			float FresnelFactor27 = fresnelNode20;
			float3 objToWorld45 = mul( unity_ObjectToWorld, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float mulTime50 = _Time.y * _ScanLineSpeed;
			float2 appendResult52 = (float2(0.5 , ( ( ( ase_worldPos.y - objToWorld45.y ) * _ScanLineTiling ) + mulTime50 )));
			float temp_output_59_0 = ( ( tex2D( _ScanLineTex, appendResult52 ).r - _ScanLineInvert ) * _ScanLinePower );
			float4 ScanLineColor55 = max( ( temp_output_59_0 * _ScanLineColor ) , float4( 0,0,0,0 ) );
			o.Emission = ( Flicking17 * ( _BaseColor + ( _BaseColor * FresnelFactor27 ) + ScanLineColor55 ) ).rgb;
			float ScanLineAlpha66 = ( temp_output_59_0 * _ScanLineAlpha );
			float clampResult38 = clamp( ( _BaseColor.a + FresnelFactor27 + ScanLineAlpha66 ) , 0.0 , 1.0 );
			float2 uv_WireFrame = i.uv_texcoord * _WireFrame_ST.xy + _WireFrame_ST.zw;
			float WireFrame32 = ( tex2D( _WireFrame, uv_WireFrame ).r * _WireFrameIntensity );
			o.Alpha = ( clampResult38 * WireFrame32 * _Alpha );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.x = customInputData.vertexToFrag135;
				o.customPack1.yz = customInputData.uv_texcoord;
				o.customPack1.yz = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.vertexToFrag135 = IN.customPack1.x;
				surfIN.uv_texcoord = IN.customPack1.yz;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
2211.2;66.4;1756.8;1016.6;3285.564;358.7734;1.522694;True;False
Node;AmplifyShaderEditor.CommentaryNode;14;-2934.023,-308.9898;Inherit;False;1749.287;416.7463;Flicking;13;41;16;42;15;9;12;13;10;11;8;6;7;135;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TransformPositionNode;7;-2884.023,-258.9897;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;54;-3286.009,1136.343;Inherit;False;2097.436;544.2279;ScanLine;18;45;62;49;47;56;53;55;59;50;46;52;58;51;44;48;57;63;68;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-2658.023,-230.9897;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-2694.023,-15.98971;Inherit;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2679.023,-106.9897;Inherit;False;Property;_Tiling;Tiling;2;0;Create;True;0;0;0;False;0;False;200;200;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;106;-1021.729,1149.114;Inherit;False;2381.954;1040.395;RandomGlicth;26;69;71;72;70;74;73;100;99;93;94;75;101;95;76;102;96;78;103;104;83;79;105;80;77;81;136;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-998.1641,1633.841;Inherit;False;Property;_RandomGlicthTiling;RandomGlicthTiling;20;0;Create;True;0;0;0;False;0;False;5;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;10;-2490.023,-233.9897;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;45;-3236.009,1341.436;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;13;-2467.023,-24.98972;Inherit;False;1;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;72;-971.164,1729.841;Inherit;False;1;0;FLOAT;-2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;69;-971.7287,1479.958;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;44;-3207.251,1186.343;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;108;-964.4388,2383.473;Inherit;False;2097.436;544.2279;ScanLineGlicth;17;123;121;120;119;118;117;116;115;114;113;112;111;110;109;128;130;134;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;-2990.531,1278.782;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;110;-914.4388,2588.566;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;12;-2280.023,-234.9897;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-3040.155,1442.01;Inherit;False;Property;_ScanLineTiling;ScanLineTiling;14;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;109;-885.6808,2433.473;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScaleAndOffsetNode;70;-727.1636,1491.841;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-3039.266,1541.477;Inherit;False;Property;_ScanLineSpeed;ScanLineSpeed;15;0;Create;True;0;0;0;False;0;False;0;-1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;74;-697.1636,1647.841;Inherit;False;1;0;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;111;-668.9609,2525.912;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-718.5849,2689.14;Inherit;False;Property;_ScanLineGlichTiling;ScanLineGlichTiling;23;0;Create;True;0;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;50;-2792.3,1501.664;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;9;-2128.023,-240.9897;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2811.815,1279.809;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-717.696,2788.607;Inherit;False;Property;_ScanLineGlichSpeed;ScanLineGlichSpeed;24;0;Create;True;0;0;0;False;0;False;0;-0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;73;-465.1638,1497.841;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-637.3987,1928.936;Inherit;True;Constant;_Float1;Float 1;21;0;Create;True;0;0;0;False;0;False;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-490.2449,2526.939;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;114;-470.73,2748.794;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;-1923.523,-453.8933;Inherit;False;flicker_vertex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-2619.075,1278.022;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-425.0448,1925.973;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;52;-2444.004,1257.12;Inherit;False;FLOAT2;4;0;FLOAT;0.5;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-313.5508,1747.661;Inherit;False;92;flicker_vertex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;116;-297.5049,2525.152;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;101;-175.8605,1931.108;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;75;-282.1638,1494.841;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-2192.455,1463.118;Inherit;False;Property;_ScanLineInvert;ScanLineInvert;16;0;Create;True;0;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;117;-122.4339,2504.25;Inherit;False;FLOAT2;4;0;FLOAT;0.5;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;28;-2521.799,163.6397;Inherit;False;1339.8;495.4;Fresnel;8;21;22;23;24;25;20;26;27;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;102;84.16922,1937.614;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;94;-112.6623,1754.152;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;53;-2290.047,1261.322;Inherit;True;Property;_ScanLineTex;ScanLineTex;13;0;Create;True;0;0;0;False;0;False;-1;None;4bbf045a9f687084ea4bc84d53c39623;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;119;65.41496,2715.448;Inherit;False;Property;_ScanLineGlichInvert;ScanLineGlichInvert;25;0;Create;True;0;0;0;False;0;False;0;0.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;76;-21.16367,1497.841;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;118;31.52294,2508.452;Inherit;True;Property;_ScanLineGlitchTex;ScanLineGlitchTex;21;0;Create;True;0;0;0;False;0;False;-1;None;afb16754b93daf04187b10b438f7a250;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-2471.799,213.6396;Inherit;True;Property;_NormalMap;NormalMap;4;0;Create;True;0;0;0;False;0;False;-1;None;77b91526e481d164aa4fee6e8b5fc94c;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;103;292.3815,1928.812;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-1998.654,1461.518;Inherit;False;Property;_ScanLinePower;ScanLinePower;17;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;56;-1949.055,1289.718;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;95;94.69547,1755.232;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;126;187.0539,2227.131;Inherit;False;Property;_ScanLineGlitchOffset;ScanLineGlitchOffset;22;0;Create;True;0;0;0;False;0;False;0,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;121;313.8162,2720.348;Inherit;False;Property;_ScanLineGlichPower;ScanLineGlichPower;26;0;Create;True;0;0;0;False;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;21;-2104.799,231.6396;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;24;-2054.799,463.64;Inherit;False;Property;_RimScale;RimScale;7;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2055.799,543.64;Inherit;False;Property;_RimPower;RimPower;6;0;Create;True;0;0;0;False;0;False;2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-1744.672,1813.132;Inherit;False;Property;_ScanLineAlpha;ScanLineAlpha;18;0;Create;True;0;0;0;False;0;False;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-2055.799,381.6398;Inherit;False;Property;_RimBias;RimBias;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;78;-285.9006,1198.464;Inherit;False;Property;_RandomVertexOffset;RandomVertexOffset;19;0;Create;True;0;0;0;False;0;False;0,0,0;-2.5,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.VertexToFragmentNode;136;443.2686,1937.295;Inherit;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;271.0321,1553.628;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;120;365.8867,2607.109;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;15;-1897.034,-230.4467;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1789.055,1297.718;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;63;-1836.599,1487.419;Inherit;False;Property;_ScanLineColor;ScanLineColor;12;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;2.286875,2.013103,2.670157,0.1490196;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;128;520.9343,2421.131;Inherit;False;Constant;_Float2;Float 2;21;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;20;-1834.202,261.1761;Inherit;False;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1904.934,-30.67319;Inherit;False;Property;_FlickCintrol;FlickCintrol;10;0;Create;True;0;0;0;False;0;False;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;104;445.5352,1621.03;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;16;-1711.189,-214.678;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;33;-1978.19,713.7151;Inherit;False;783.4757;368.563;WireFrame;4;30;31;29;32;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;79;25.83632,1394.841;Inherit;False;Constant;_Float0;Float 0;21;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;127;457.5987,2225.405;Inherit;False;View;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-1514.672,1770.132;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TransformDirectionNode;83;-37.49924,1199.114;Inherit;False;View;Object;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1568.457,1294.983;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;527.2123,2634.995;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;68;-1413.45,1295.362;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;134;689.9551,2613.833;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1839.59,966.8781;Inherit;False;Property;_WireFrameIntensity;WireFrameIntensity;9;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-1928.19,763.7151;Inherit;True;Property;_WireFrame;WireFrame;8;0;Create;True;0;0;0;False;0;False;-1;None;668fcaed21c1ad143a5b2782b04ad025;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;228.8361,1299.841;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;41;-1541.934,-215.6732;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;66;-1363.672,1771.132;Inherit;False;ScanLineAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;129;723.9341,2326.131;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;105;625.738,1548.803;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-1510.73,255.1901;Inherit;False;FresnelFactor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-1260.216,1307.636;Inherit;False;ScanLineColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;3;-1023.987,184.5368;Inherit;False;Property;_BaseColor;BaseColor;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;0,1.254902,2.996078,0.1490196;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;130;916.7335,2492.663;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;135;-1383.603,-214.9692;Inherit;False;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1593.655,824.8167;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;869.1105,1410.532;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;34;-971.6252,500.7374;Inherit;False;27;FresnelFactor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;67;-965.822,605.4971;Inherit;False;66;ScanLineAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;32;-1419.514,824.8168;Inherit;False;WireFrame;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;1083.705,1415.669;Inherit;False;RandomGlicth;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-1161.406,-216.4145;Inherit;False;Flicking;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-684.1122,229.8566;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-691.4883,478.9491;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;131;1067.778,2497.872;Inherit;False;ScanLineGlitch;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-697.9818,359.8134;Inherit;False;55;ScanLineColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-323.8831,711.5936;Inherit;False;81;RandomGlicth;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;133;-324.273,805.0682;Inherit;False;131;ScanLineGlitch;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-587.0691,625.8997;Inherit;False;32;WireFrame;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-673.188,728.416;Inherit;False;Property;_Alpha;Alpha;11;0;Create;True;0;0;0;False;0;False;0;0.3;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;38;-528.593,477.9114;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;18;-717.4885,22.26307;Inherit;False;17;Flicking;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-524.331,164.4913;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;26;-1593.032,411.2494;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;132;-103.6311,716.0837;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1532.183,-637.2762;Inherit;False;Property;_ZWriteMode;ZWriteMode;3;1;[Toggle];Create;True;0;1;Option1;0;1;;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-383.6456,53.2365;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-350.135,513.188;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Hologram;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;True;19;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;7;1
WireConnection;8;1;7;2
WireConnection;8;2;7;3
WireConnection;10;0;8;0
WireConnection;10;1;11;0
WireConnection;10;2;6;0
WireConnection;46;0;44;2
WireConnection;46;1;45;2
WireConnection;12;0;10;0
WireConnection;12;1;13;0
WireConnection;70;0;69;2
WireConnection;70;1;71;0
WireConnection;70;2;72;0
WireConnection;111;0;109;2
WireConnection;111;1;110;2
WireConnection;50;0;49;0
WireConnection;9;0;12;0
WireConnection;47;0;46;0
WireConnection;47;1;48;0
WireConnection;73;0;70;0
WireConnection;73;1;74;0
WireConnection;115;0;111;0
WireConnection;115;1;113;0
WireConnection;114;0;112;0
WireConnection;92;0;9;0
WireConnection;51;0;47;0
WireConnection;51;1;50;0
WireConnection;99;0;73;0
WireConnection;99;1;100;0
WireConnection;52;1;51;0
WireConnection;116;0;115;0
WireConnection;116;1;114;0
WireConnection;101;0;99;0
WireConnection;75;0;73;0
WireConnection;117;1;116;0
WireConnection;102;0;101;0
WireConnection;94;0;93;0
WireConnection;53;1;52;0
WireConnection;76;0;75;0
WireConnection;118;1;117;0
WireConnection;103;0;102;0
WireConnection;56;0;53;1
WireConnection;56;1;57;0
WireConnection;95;0;94;0
WireConnection;21;0;22;0
WireConnection;136;0;103;0
WireConnection;96;0;76;0
WireConnection;96;1;95;0
WireConnection;120;0;118;1
WireConnection;120;1;119;0
WireConnection;15;0;9;0
WireConnection;59;0;56;0
WireConnection;59;1;58;0
WireConnection;20;0;21;0
WireConnection;20;1;23;0
WireConnection;20;2;24;0
WireConnection;20;3;25;0
WireConnection;104;0;96;0
WireConnection;104;1;136;0
WireConnection;16;0;15;0
WireConnection;127;0;126;0
WireConnection;65;0;59;0
WireConnection;65;1;64;0
WireConnection;83;0;78;0
WireConnection;62;0;59;0
WireConnection;62;1;63;0
WireConnection;123;0;120;0
WireConnection;123;1;121;0
WireConnection;68;0;62;0
WireConnection;134;0;123;0
WireConnection;80;0;83;0
WireConnection;80;1;79;0
WireConnection;41;1;16;0
WireConnection;41;2;42;0
WireConnection;66;0;65;0
WireConnection;129;0;127;0
WireConnection;129;1;128;0
WireConnection;105;0;96;0
WireConnection;105;1;104;0
WireConnection;27;0;20;0
WireConnection;55;0;68;0
WireConnection;130;0;129;0
WireConnection;130;1;134;0
WireConnection;135;0;41;0
WireConnection;30;0;29;1
WireConnection;30;1;31;0
WireConnection;77;0;80;0
WireConnection;77;1;105;0
WireConnection;32;0;30;0
WireConnection;81;0;77;0
WireConnection;17;0;135;0
WireConnection;35;0;3;0
WireConnection;35;1;34;0
WireConnection;37;0;3;4
WireConnection;37;1;34;0
WireConnection;37;2;67;0
WireConnection;131;0;130;0
WireConnection;38;0;37;0
WireConnection;36;0;3;0
WireConnection;36;1;35;0
WireConnection;36;2;60;0
WireConnection;132;0;82;0
WireConnection;132;1;133;0
WireConnection;2;0;18;0
WireConnection;2;1;36;0
WireConnection;39;0;38;0
WireConnection;39;1;40;0
WireConnection;39;2;43;0
WireConnection;0;2;2;0
WireConnection;0;9;39;0
WireConnection;0;11;132;0
ASEEND*/
//CHKSM=70B66117F2B9AE44AEA7327D3F1A95794E3BD6F6