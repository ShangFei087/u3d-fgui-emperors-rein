// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Liquid"
{
	Properties
	{
		_Float3("液面摆动X", Float) = 0
		_Float5("液面摆动Z", Float) = 0
		_Float8("液面高度", Range( 0 , 1)) = 0
		_Vector5("液面重映射", Vector) = (0,1,2.2,-1.3)
		_Float7("夜浪密度", Float) = 1
		_Float11("波浪振幅", Float) = 0.5
		_Float9("液面速度", Float) = 1
		_MainTex("MainTex", 2D) = "white" {}
		_Main_X("主纹理速度X", Float) = 0
		_Main_Y("主纹理速度Y", Float) = 0
		_MainIntensity("主纹理亮度", Float) = 0
		_ParallaxTexA("视差纹理A", 2D) = "white" {}
		_Vector1("A_xy密度_zw速度", Vector) = (0,0,0,0)
		_Float1("A亮度", Float) = 1
		__ParallaxTexB("视差纹理B", 2D) = "white" {}
		_Vector7("B_xy密度_zw速度", Vector) = (0,0,0,0)
		_Float2("B亮度", Float) = 1
		_Vector6("视差折射率", Range( 0 , 3)) = 0
		_Vector0("Fresnel", Vector) = (0,1,5,0)
		[HDR]_FresnelColor("FresnelColor", Color) = (0,0,0,0)
		[HDR]_ParallaxBColor("液面加颜色", Color) = (0,0,0,0)
		_Vector2("液面纹理XY密度zw速度", Vector) = (0,0,0,0)
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0
		[Enum(on,0,off,1)]_Toggle("深度开关", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite [_Toggle]
		ZTest LEqual
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
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_POSITION


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
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
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Toggle;
			uniform float _CullMode;
			uniform sampler2D _ParallaxTexA;
			uniform float4 _Vector1;
			uniform float _Vector6;
			uniform float _Float1;
			uniform sampler2D __ParallaxTexB;
			uniform float4 _Vector7;
			uniform float _Float2;
			uniform float3 _Vector0;
			uniform float4 _FresnelColor;
			uniform sampler2D _MainTex;
			uniform float _Main_X;
			uniform float _Main_Y;
			uniform float _MainIntensity;
			uniform float4 _Vector2;
			uniform float4 _ParallaxBColor;
			uniform float _Float8;
			uniform float4 _Vector5;
			uniform float _Float3;
			uniform float _Float5;
			uniform float _Float7;
			uniform float _Float9;
			uniform float _Float11;
			float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
			{
				original -= center;
				float C = cos( angle );
				float S = sin( angle );
				float t = 1 - C;
				float m00 = t * u.x * u.x + C;
				float m01 = t * u.x * u.y - S * u.z;
				float m02 = t * u.x * u.z + S * u.y;
				float m10 = t * u.x * u.y + S * u.z;
				float m11 = t * u.y * u.y + C;
				float m12 = t * u.y * u.z - S * u.x;
				float m20 = t * u.x * u.z - S * u.y;
				float m21 = t * u.y * u.z + S * u.x;
				float m22 = t * u.z * u.z + C;
				float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
				return mul( finalMatrix, original ) + center;
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldTangent = UnityObjectToWorldDir(v.ase_tangent);
				o.ase_texcoord2.xyz = ase_worldTangent;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord4.xyz = ase_worldBitangent;
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord5 = screenPos;
				
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_texcoord6 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
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
			
			fixed4 frag (v2f i , half ase_vface : VFACE) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 appendResult40 = (float2(_Vector1.z , _Vector1.w));
				float2 texCoord36 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult39 = (float2(_Vector1.x , _Vector1.y));
				float2 panner41 = ( 1.0 * _Time.y * appendResult40 + ( texCoord36 * appendResult39 ));
				float3 ase_worldTangent = i.ase_texcoord2.xyz;
				float3 ase_worldNormal = i.ase_texcoord3.xyz;
				float3 ase_worldBitangent = i.ase_texcoord4.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_tanViewDir =  tanToWorld0 * ase_worldViewDir.x + tanToWorld1 * ase_worldViewDir.y  + tanToWorld2 * ase_worldViewDir.z;
				ase_tanViewDir = normalize(ase_tanViewDir);
				float3 normalizeResult23 = normalize( ase_tanViewDir );
				float3x3 ase_worldToTangent = float3x3(ase_worldTangent,ase_worldBitangent,ase_worldNormal);
				float3 worldToTangentPos25 = mul( ase_worldToTangent, ase_worldNormal);
				float3 normalizeResult26 = normalize( worldToTangentPos25 );
				float3 break31 = refract( normalizeResult23 , normalizeResult26 , ( 1.0 / _Vector6 ) );
				float2 appendResult32 = (float2(break31.x , break31.y));
				float2 Parallax34 = ( appendResult32 / break31.z );
				float2 appendResult46 = (float2(_Vector7.z , _Vector7.w));
				float2 appendResult45 = (float2(_Vector7.x , _Vector7.y));
				float2 panner48 = ( 1.0 * _Time.y * appendResult46 + ( texCoord36 * appendResult45 ));
				float4 AB61 = ( ( tex2D( _ParallaxTexA, ( panner41 + Parallax34 ) ) * _Float1 ) + ( tex2D( __ParallaxTexB, ( Parallax34 + panner48 ) ) * _Float2 ) );
				float fresnelNdotV5 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode5 = ( _Vector0.x + _Vector0.y * pow( 1.0 - fresnelNdotV5, _Vector0.z ) );
				float2 appendResult14 = (float2(_Main_X , _Main_Y));
				float2 texCoord2 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 panner3 = ( 1.0 * _Time.y * appendResult14 + texCoord2);
				float4 tex2DNode1 = tex2D( _MainTex, panner3 );
				float2 appendResult68 = (float2(_Vector2.z , _Vector2.w));
				float4 screenPos = i.ase_texcoord5;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float2 appendResult67 = (float2(_Vector2.x , _Vector2.y));
				float2 panner70 = ( 1.0 * _Time.y * appendResult68 + ( (ase_screenPosNorm).xy * appendResult67 ));
				float4 Back74 = ( tex2D( __ParallaxTexB, panner70 ) + _ParallaxBColor );
				float4 switchResult13 = (((ase_vface>0)?(( AB61 + ( ( fresnelNode5 * _FresnelColor ) + ( ( tex2DNode1 * tex2DNode1.a * _MainIntensity ) * float4( 0,0,0,0 ) ) ) )):(Back74)));
				float4 transform78 = mul(unity_ObjectToWorld,float4(0,0,0,1));
				float3 rotatedValue85 = RotateAroundAxis( float3( 0,0,0 ), i.ase_texcoord6.xyz, float3(-1,0,0), 90.0 );
				float3 break90 = ( ( WorldPosition - (transform78).xyz ) + ( _Float3 * i.ase_texcoord6.xyz ) + ( rotatedValue85 * _Float5 ) );
				float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
				float3 temp_output_91_0 = ( 1.0 / ase_objectScale );
				float2 appendResult96 = (float2(break90.x , break90.z));
				float3 temp_cast_1 = (0.5).xxx;
				float3 break112 = ( ( sin( ( ( float3( appendResult96 ,  0.0 ) * temp_output_91_0 * _Float7 ) + ( _Time.y * _Float9 ) ) ) - temp_cast_1 ) * _Float11 );
				float Clip117 = step( ( ( (_Vector5.z + (_Float8 - _Vector5.x) * (_Vector5.w - _Vector5.z) / (_Vector5.y - _Vector5.x)) + ( break90.y * (temp_output_91_0).y ) ) + ( break112.x * break112.y ) ) , 0.5 );
				float4 appendResult128 = (float4((switchResult13).rgb , Clip117));
				
				
				finalColor = appendResult128;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
2100;89.6;1910.4;934.2;1627.623;966.5068;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;121;-1999.09,-20.13963;Inherit;False;3624.25;978.1942;Comment;42;76;78;86;81;87;85;89;79;83;77;88;82;80;84;93;92;90;97;105;91;104;96;95;103;106;109;107;111;108;102;101;94;110;100;112;98;113;99;116;114;115;117;液面裁切;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;35;-1525.971,-2264.599;Inherit;False;1680.103;632.0013;Parallax;13;30;31;25;28;27;33;32;23;26;24;22;29;34;视差UV计算;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;76;-1950.09,203.8133;Inherit;False;Constant;_Vector3;Vector 3;15;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;24;-1475.971,-2014.397;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;30;-1334.718,-1745.302;Inherit;False;Property;_Vector6;视差折射率;17;0;Create;False;0;0;0;False;0;False;0;3;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1264.469,-1846.798;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;81;-1581.163,435.8604;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;86;-1671.5,621.6547;Inherit;False;Constant;_Vector4;Vector 4;16;0;Create;True;0;0;0;False;0;False;-1,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;87;-1654.6,776.3548;Inherit;False;Constant;_Float4;Float 4;16;0;Create;True;0;0;0;False;0;False;90;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;78;-1763.163,208.8604;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;22;-1470.771,-2214.599;Inherit;False;Tangent;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;25;-1258.87,-2027.398;Inherit;False;World;Tangent;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;83;-1562.163,354.8604;Inherit;False;Property;_Float3;液面摆动X;0;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;85;-1330.899,634.6544;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;79;-1564.163,208.8604;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;77;-1777.163,29.86037;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;26;-1048.269,-2035.197;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;23;-1254.97,-2213.299;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-1023.97,-1827.298;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-1204.799,784.1546;Inherit;False;Property;_Float5;液面摆动Z;1;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;82;-1343.163,390.8604;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RefractOpVec;27;-836.3691,-2179.499;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;80;-1362.163,166.8604;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-1020.199,634.6545;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;-898.7632,218.8604;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;31;-604.9675,-2186.002;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CommentaryNode;63;-1621.342,-1530.848;Inherit;False;2329.17;819.6367;Comment;23;49;45;38;36;51;48;61;40;55;58;60;47;39;52;62;43;42;46;54;44;41;50;37;液体正面双视差AB加颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;38;-1571.342,-1261.582;Inherit;False;Property;_Vector1;A_xy密度_zw速度;12;0;Create;False;0;0;0;False;0;False;0,0,0,0;1,1,0.1,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;90;-741.9953,223.8546;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ObjectScaleNode;93;-869.9382,543.655;Inherit;False;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;92;-877.1952,422.7548;Inherit;False;Constant;_Float6;Float 6;17;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;32;-409.9673,-2199.002;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;44;-1549.468,-964.3585;Inherit;False;Property;_Vector7;B_xy密度_zw速度;15;0;Create;False;0;0;0;False;0;False;0,0,0,0;1,1,0.05,0.05;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;36;-1540.403,-1480.848;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;96;-479.0708,447.8863;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;45;-1089.062,-998.5664;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;91;-697.3636,457.7462;Inherit;False;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;33;-253.9674,-2161.301;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;97;-484.5927,652.8549;Inherit;False;Property;_Float7;夜浪密度;4;0;Create;False;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-479.3911,842.6545;Inherit;False;Property;_Float9;液面速度;6;0;Create;False;0;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;104;-514.4907,755.5547;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;39;-1339.057,-1286.117;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-1096.344,-1464.468;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-907.0701,-1045.885;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;40;-1332.933,-1151.442;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1085.423,-847.5135;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-70.6676,-2152.201;Inherit;False;Parallax;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;75;-1602.608,-616.5187;Inherit;False;1652.04;519.8376;Comment;11;72;68;64;71;66;65;69;70;67;73;74;背面/水面/基于屏幕空间的纹理;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-266.1907,769.8544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-290.8937,565.7545;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1185.776,1745.063;Inherit;False;Property;_Main_X;主纹理速度X;8;0;Create;False;0;0;0;False;0;False;0;0.02;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-93.29126,593.0546;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;66;-1552.608,-338.4811;Inherit;False;Property;_Vector2;液面纹理XY密度zw速度;21;0;Create;False;0;0;0;False;0;False;0,0,0,0;5,5,0.1,-0.01;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;43;-726.8979,-1233.339;Inherit;False;34;Parallax;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;41;-741.4583,-1391.673;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;48;-719.6202,-907.5713;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;64;-1531.76,-558.782;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;50;-723.2598,-1025.865;Inherit;False;34;Parallax;1;0;OBJECT;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1182.576,1823.463;Inherit;False;Property;_Main_Y;主纹理速度Y;9;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;107;110.8087,568.3546;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ComponentMaskNode;65;-1301.608,-547.4811;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;-1028.975,1754.663;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;-493.9504,-978.5471;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;67;-1282.608,-339.4811;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-1192.849,1599.719;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;42;-503.0475,-1371.652;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;109;113.4409,697.5611;Inherit;False;Constant;_Float10;Float 10;21;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-1066.608,-546.4811;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-159.0691,-1197.23;Inherit;False;Property;_Float1;A亮度;13;0;Create;False;0;0;0;False;0;False;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;108;321.9698,579.5933;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;51;-297.9583,-1405.788;Inherit;True;Property;_ParallaxTexA;视差纹理A;11;0;Create;False;0;0;0;False;0;False;-1;None;974e3d69c4b93724f982b25b5cf34539;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;3;-926.7492,1605.419;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;62;-299.3712,-1050.155;Inherit;True;Property;_ParallaxTex1;视差纹理;14;0;Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;71;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;60;-169.5432,-847.411;Inherit;False;Property;_Float2;B亮度;16;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;68;-1281.608,-230.4811;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;111;344.61,709.4769;Inherit;False;Property;_Float11;波浪振幅;5;0;Create;False;0;0;0;False;0;False;0.5;0.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;70;-887.6077,-538.4811;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;101;-448.2809,141.5949;Inherit;False;Property;_Vector5;液面重映射;3;0;Create;False;0;0;0;False;0;False;0,1,2.2,-1.3;0,1,0.54,0.47;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-708.6492,1600.219;Inherit;True;Property;_MainTex;MainTex;7;0;Create;True;0;0;0;False;0;False;-1;None;974e3d69c4b93724f982b25b5cf34539;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;102;-521.2625,40.59885;Inherit;False;Property;_Float8;液面高度;2;0;Create;False;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;98.72518,-1365.192;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;94;-452.0883,354.2903;Inherit;False;False;True;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;6;-1109.248,1242.518;Inherit;False;Property;_Vector0;Fresnel;18;0;Create;False;0;0;0;False;0;False;0,1,5;0,1,3;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;17;-577.5449,1802.332;Inherit;False;Property;_MainIntensity;主纹理亮度;10;0;Create;False;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;551.9471,590.3176;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;46.22824,-975.411;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-143.1235,315.5061;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;-691.6819,-566.5187;Inherit;True;Property;__ParallaxTexB;视差纹理B;14;0;Create;False;0;0;0;False;0;False;-1;None;974e3d69c4b93724f982b25b5cf34539;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;5;-853.7481,1225.918;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;73;-630.8475,-356.5331;Inherit;False;Property;_ParallaxBColor;液面加颜色;20;1;[HDR];Create;False;0;0;0;False;0;False;0,0,0,0;0.5,0.5,0.5,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;100;-193.3925,130.2545;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;264.1895,-1232.459;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;112;721.1533,589.1262;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-332.8485,1621.419;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;7;-808.4565,1392.619;Inherit;False;Property;_FresnelColor;FresnelColor;19;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;1,0.4166667,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-337.6224,-531.5603;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-522.0479,1304.219;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-121.2439,1637.232;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;887.9759,604.6169;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;483.0278,-1217.012;Inherit;False;AB;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;67.90751,268.0544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-175.3676,-527.1742;Inherit;False;Back;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;116;1046.458,614.1495;Inherit;False;Constant;_Float12;Float 12;22;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;32.05188,1444.318;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;114;1021.434,443.7518;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;11;17.42691,1354.263;Inherit;False;61;AB;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;10;212.3268,1405.763;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;19;190.7567,1541.031;Inherit;False;74;Back;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;115;1209.706,442.5602;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;13;374.6257,1407.763;Inherit;False;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;1400.36,443.7518;Inherit;False;Clip;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;127;556.0253,1433.191;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;411.0606,1670.442;Inherit;False;117;Clip;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;430.2532,1788.542;Inherit;False;Constant;_Float13;Float 13;23;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-1461.663,2156.975;Inherit;False;Property;_Toggle;深度开关;23;1;[Enum];Create;False;0;2;on;0;off;1;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-1467.069,2044.711;Inherit;False;Property;_CullMode;剔除模式;22;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;12;-353.8738,1752.315;Inherit;False;117;Clip;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;128;811.0253,1451.191;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;126;980.7909,1467.761;Float;False;True;-1;2;ASEMaterialInspector;100;1;Liquid;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;True;129;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;True;130;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;78;0;76;0
WireConnection;25;0;24;0
WireConnection;85;0;86;0
WireConnection;85;1;87;0
WireConnection;85;3;81;0
WireConnection;79;0;78;0
WireConnection;26;0;25;0
WireConnection;23;0;22;0
WireConnection;28;0;29;0
WireConnection;28;1;30;0
WireConnection;82;0;83;0
WireConnection;82;1;81;0
WireConnection;27;0;23;0
WireConnection;27;1;26;0
WireConnection;27;2;28;0
WireConnection;80;0;77;0
WireConnection;80;1;79;0
WireConnection;88;0;85;0
WireConnection;88;1;89;0
WireConnection;84;0;80;0
WireConnection;84;1;82;0
WireConnection;84;2;88;0
WireConnection;31;0;27;0
WireConnection;90;0;84;0
WireConnection;32;0;31;0
WireConnection;32;1;31;1
WireConnection;96;0;90;0
WireConnection;96;1;90;2
WireConnection;45;0;44;1
WireConnection;45;1;44;2
WireConnection;91;0;92;0
WireConnection;91;1;93;0
WireConnection;33;0;32;0
WireConnection;33;1;31;2
WireConnection;39;0;38;1
WireConnection;39;1;38;2
WireConnection;37;0;36;0
WireConnection;37;1;39;0
WireConnection;47;0;36;0
WireConnection;47;1;45;0
WireConnection;40;0;38;3
WireConnection;40;1;38;4
WireConnection;46;0;44;3
WireConnection;46;1;44;4
WireConnection;34;0;33;0
WireConnection;103;0;104;0
WireConnection;103;1;105;0
WireConnection;95;0;96;0
WireConnection;95;1;91;0
WireConnection;95;2;97;0
WireConnection;106;0;95;0
WireConnection;106;1;103;0
WireConnection;41;0;37;0
WireConnection;41;2;40;0
WireConnection;48;0;47;0
WireConnection;48;2;46;0
WireConnection;107;0;106;0
WireConnection;65;0;64;0
WireConnection;14;0;15;0
WireConnection;14;1;16;0
WireConnection;49;0;50;0
WireConnection;49;1;48;0
WireConnection;67;0;66;1
WireConnection;67;1;66;2
WireConnection;42;0;41;0
WireConnection;42;1;43;0
WireConnection;69;0;65;0
WireConnection;69;1;67;0
WireConnection;108;0;107;0
WireConnection;108;1;109;0
WireConnection;51;1;42;0
WireConnection;3;0;2;0
WireConnection;3;2;14;0
WireConnection;62;1;49;0
WireConnection;68;0;66;3
WireConnection;68;1;66;4
WireConnection;70;0;69;0
WireConnection;70;2;68;0
WireConnection;1;1;3;0
WireConnection;52;0;51;0
WireConnection;52;1;54;0
WireConnection;94;0;91;0
WireConnection;110;0;108;0
WireConnection;110;1;111;0
WireConnection;58;0;62;0
WireConnection;58;1;60;0
WireConnection;98;0;90;1
WireConnection;98;1;94;0
WireConnection;71;1;70;0
WireConnection;5;1;6;1
WireConnection;5;2;6;2
WireConnection;5;3;6;3
WireConnection;100;0;102;0
WireConnection;100;1;101;1
WireConnection;100;2;101;2
WireConnection;100;3;101;3
WireConnection;100;4;101;4
WireConnection;55;0;52;0
WireConnection;55;1;58;0
WireConnection;112;0;110;0
WireConnection;4;0;1;0
WireConnection;4;1;1;4
WireConnection;4;2;17;0
WireConnection;72;0;71;0
WireConnection;72;1;73;0
WireConnection;8;0;5;0
WireConnection;8;1;7;0
WireConnection;18;0;4;0
WireConnection;113;0;112;0
WireConnection;113;1;112;1
WireConnection;61;0;55;0
WireConnection;99;0;100;0
WireConnection;99;1;98;0
WireConnection;74;0;72;0
WireConnection;9;0;8;0
WireConnection;9;1;18;0
WireConnection;114;0;99;0
WireConnection;114;1;113;0
WireConnection;10;0;11;0
WireConnection;10;1;9;0
WireConnection;115;0;114;0
WireConnection;115;1;116;0
WireConnection;13;0;10;0
WireConnection;13;1;19;0
WireConnection;117;0;115;0
WireConnection;127;0;13;0
WireConnection;128;0;127;0
WireConnection;128;3;120;0
WireConnection;126;0;128;0
ASEEND*/
//CHKSM=EB73DF648473F53EEC45B4D6E936F54D212B0B51