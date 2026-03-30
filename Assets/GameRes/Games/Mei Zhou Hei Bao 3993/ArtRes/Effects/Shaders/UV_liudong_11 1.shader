// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "XuanFu/Particles/UV_liudong_11"
{
	Properties
	{
		_Mask_01("Mask_01", 2D) = "white" {}
		_AddTex("AddTex", 2D) = "black" {}
		[HDR]_Color0("Color 0", Color) = (4,1.05098,1.003922,1)
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_noise_diss_TEX("noise_diss_TEX", 2D) = "white" {}
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_diss_Speed("diss_Speed", Vector) = (0,0.1,0,0)
		_Te_Speed("Te_Speed", Vector) = (-0.57,0,0,0)
		_noise_diss_speed("noise_diss_speed", Vector) = (-0.47,0.45,0,0)
		_Noise_Speed("Noise_Speed", Vector) = (0,0,0,0)
		_NoiseScale1("扰动", Range( 0 , 0.5)) = 0.1153234
		_noise_diss_INT("noise_diss_INT", Range( 0 , 0.5)) = 0.06032216
		_or("or", Range( 0 , 1)) = 0
		_smoothstep("smoothstep", Range( 0 , 1)) = 1
		_zezao_power("zezao_power", Float) = 0.71

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
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


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
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
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _Mask_01;
			uniform float4 _Mask_01_ST;
			uniform sampler2D _AddTex;
			uniform float2 _Te_Speed;
			uniform float4 _AddTex_ST;
			uniform sampler2D _NoiseTex;
			uniform float2 _Noise_Speed;
			uniform float4 _NoiseTex_ST;
			uniform float _NoiseScale1;
			uniform float4 _Color0;
			uniform float _smoothstep;
			uniform float _or;
			uniform float _zezao_power;
			uniform sampler2D _DissolveTex;
			uniform float2 _diss_Speed;
			uniform float4 _DissolveTex_ST;
			uniform sampler2D _noise_diss_TEX;
			uniform float2 _noise_diss_speed;
			uniform float4 _noise_diss_TEX_ST;
			uniform float _noise_diss_INT;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1 = v.ase_texcoord;
				o.ase_color = v.color;
				o.ase_texcoord2 = v.ase_texcoord1;
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
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_Mask_01 = i.ase_texcoord1.xy * _Mask_01_ST.xy + _Mask_01_ST.zw;
				float4 texCoord237 = i.ase_texcoord1;
				texCoord237.xy = i.ase_texcoord1.xy * float2( 0,0 ) + float2( 0,0 );
				float2 appendResult235 = (float2(texCoord237.z , texCoord237.w));
				float2 temp_output_232_0 = ( uv_Mask_01 + appendResult235 );
				float4 tex2DNode201 = tex2D( _Mask_01, temp_output_232_0 );
				float4 temp_cast_0 = (1.0).xxxx;
				float2 appendResult170 = (float2(_Te_Speed.x , _Te_Speed.y));
				float2 uv_AddTex = i.ase_texcoord1.xy * _AddTex_ST.xy + _AddTex_ST.zw;
				float2 panner182 = ( 1.0 * _Time.y * appendResult170 + uv_AddTex);
				float2 appendResult161 = (float2(_Noise_Speed.x , _Noise_Speed.y));
				float2 uv_NoiseTex = i.ase_texcoord1.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 panner174 = ( 1.0 * _Time.y * appendResult161 + uv_NoiseTex);
				float2 temp_cast_1 = (tex2D( _NoiseTex, panner174 ).r).xx;
				float2 lerpResult186 = lerp( panner182 , temp_cast_1 , _NoiseScale1);
				float2 texCoord167 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult180 = lerp( texCoord167.x , texCoord167.y , _or);
				float temp_output_184_0 = ( lerpResult180 + 0.0 );
				float2 appendResult168 = (float2(_diss_Speed.x , _diss_Speed.y));
				float2 uv_DissolveTex = i.ase_texcoord1.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float2 panner179 = ( 1.0 * _Time.y * appendResult168 + uv_DissolveTex);
				float2 appendResult155 = (float2(_noise_diss_speed.x , _noise_diss_speed.y));
				float2 uv_noise_diss_TEX = i.ase_texcoord1.xy * _noise_diss_TEX_ST.xy + _noise_diss_TEX_ST.zw;
				float2 panner172 = ( 1.0 * _Time.y * appendResult155 + uv_noise_diss_TEX);
				float4 lerpResult183 = lerp( float4( panner179, 0.0 , 0.0 ) , tex2D( _noise_diss_TEX, panner172 ) , _noise_diss_INT);
				float4 texCoord250 = i.ase_texcoord2;
				texCoord250.xy = i.ase_texcoord2.xy * float2( 0,0 ) + float2( 0,0 );
				float smoothstepResult206 = smoothstep( 0.0 , _smoothstep , ( ( pow( temp_output_184_0 , _zezao_power ) + tex2D( _DissolveTex, lerpResult183.rg ).r ) + ( texCoord250.z * -2.0 ) + 1.0 ));
				
				
				finalColor = ( ( pow( tex2DNode201 , temp_cast_0 ) * ( tex2DNode201.a * ( tex2D( _AddTex, lerpResult186 ) * _Color0 ) ) ) * (0.0 + (i.ase_color.a - 0.0) * (1.0 - 0.0) / (1.0 - 0.0)) * saturate( smoothstepResult206 ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18912
2010;89;1458;1320;-1033.92;-837.8125;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;238;-1676.263,913.3904;Inherit;False;4005.3;1272.241;Comment;26;157;158;195;166;167;180;184;188;190;192;211;213;154;155;163;172;176;156;164;168;175;179;183;215;214;250;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;154;-1592.349,1952.341;Inherit;False;Property;_noise_diss_speed;noise_diss_speed;13;0;Create;True;0;0;0;False;0;False;-0.47,0.45;-0.47,0.45;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;155;-1314.717,1950.247;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;163;-1626.263,1742.057;Inherit;False;0;176;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;159;-1898.423,-593.6304;Inherit;False;Property;_Noise_Speed;Noise_Speed;14;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;164;-1106.941,1707.97;Inherit;False;Property;_diss_Speed;diss_Speed;10;0;Create;True;0;0;0;False;0;False;0,0.1;0,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode;161;-1620.791,-593.4676;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;168;-829.3087,1709.132;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;175;-1139.555,1504.735;Inherit;False;0;192;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;166;-626.5712,1406.638;Inherit;False;Property;_or;or;22;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;165;-1932.337,-801.6576;Inherit;False;0;177;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;162;-1575.988,-1071.104;Inherit;False;Property;_Te_Speed;Te_Speed;11;0;Create;True;0;0;0;False;0;False;-0.57,0;0.5,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;167;-771.0405,1097.233;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;172;-1041.83,1821.908;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;174;-1347.904,-722.8065;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;156;-709.8798,2070.631;Inherit;False;Property;_noise_diss_INT;noise_diss_INT;20;0;Create;True;0;0;0;False;0;False;0.06032216;0.06032216;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;179;-548.8364,1574.105;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;170;-1303.988,-1071.104;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;169;-1607.988,-1279.104;Inherit;False;0;189;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;176;-748.6488,1780.878;Inherit;True;Property;_noise_diss_TEX;noise_diss_TEX;6;0;Create;True;0;0;0;False;0;False;-1;None;c85f4c7ad32051742a569fdc1db638af;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;180;-335.7907,1189.512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;177;-1055.723,-763.8366;Inherit;True;Property;_NoiseTex;NoiseTex;7;0;Create;True;0;0;0;False;0;False;-1;None;c85f4c7ad32051742a569fdc1db638af;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;188;148.4821,1381.544;Inherit;False;Property;_zezao_power;zezao_power;25;0;Create;True;0;0;0;False;0;False;0.71;0.71;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;183;-164.5358,1634.367;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;157;794.5566,1207.981;Inherit;False;656.5996;502.7548;ruan;5;204;200;198;194;191;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;184;-180.0476,1112.325;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;237;-1653.043,-2503.404;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;182;-1031.988,-1199.104;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;178;-911.8734,-562.1772;Inherit;False;Property;_NoiseScale1;扰动;18;0;Create;False;0;0;0;False;0;False;0.1153234;0.1153234;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;234;-1171.453,-2601.165;Inherit;False;0;201;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;235;-1044.167,-2362.167;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;186;-548.476,-869.7801;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;250;567.4825,1276.491;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;190;313.0997,1139.548;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;192;294.966,1513.904;Inherit;True;Property;_DissolveTex;DissolveTex;5;0;Create;True;0;0;0;False;0;False;-1;None;70f2dc0d7be915c4e9a9780763338a42;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;194;934.6647,1594.736;Inherit;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;195;628.7643,963.3904;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;1176.664,1473.737;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;232;-554.5742,-2390.799;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;198;1136.419,1357.39;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;248;-251.2646,-747.7288;Inherit;False;Property;_Color0;Color 0;4;1;[HDR];Create;True;0;0;0;False;0;False;4,1.05098,1.003922,1;4,1.867202,1.005235,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;158;1553.065,1217.145;Inherit;False;384.1431;335.5599;or yin;2;206;205;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;189;-343.7095,-1067.962;Inherit;True;Property;_AddTex;AddTex;2;0;Create;True;0;0;0;False;0;False;-1;8a3c4a5c38bc89442af266500734c9bb;fca9e1d0cf0a79445bec95a682d1896e;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;203;1242.597,-1442.594;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;201;-60.64551,-2037.466;Inherit;True;Property;_Mask_01;Mask_01;1;0;Create;True;0;0;0;False;0;False;-1;09c234aa5871c274cbf9afb4a328797b;0ea89dd1dbf45a540be144c829de3416;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;204;1298.156,1257.981;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;246;486.0847,-1614.117;Inherit;False;Constant;_Float3;Float 3;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;1603.065,1436.704;Inherit;False;Property;_smoothstep;smoothstep;23;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;249;52.21826,-953.5817;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;209;416.2843,-1070.544;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;208;1453.221,-1404.956;Inherit;False;FLOAT;1;0;FLOAT;0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SmoothstepOpNode;206;1748.208,1267.145;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;247;617.2062,-1848.512;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;210;1601.019,-1394.721;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;212;1107.361,-975.4319;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;211;2131.037,1164.302;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;181;-1474.71,-68.74858;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;225;-1648.893,-1747.776;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;234.9426,77.44794;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-600.308,61.44946;Inherit;False;Constant;_Float0;Float 0;15;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;185;-1002.826,-162.7661;Inherit;True;Property;_AddTex_02;AddTex_02;0;0;Create;True;0;0;0;False;0;False;-1;None;5a1708cb1950f9c4c9369166510e04dc;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;216;2032.651,-656.8112;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-2059.145,-146.6006;Inherit;False;0;185;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;240;-1296.628,566.6049;Inherit;False;Property;_NoiseScale3;扰动2;17;0;Create;False;0;0;0;False;0;False;0.1153234;0.096;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;241;-2477.148,581.765;Inherit;False;Property;_Noisespeed_02;Noisespeed_02;15;0;Create;False;0;0;0;False;0;False;0,0;0,0.11;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;228;-1028.348,-1934.536;Inherit;True;Property;_mask_NoiseTex1;mask_NoiseTex;9;0;Create;False;0;0;0;False;0;False;-1;None;c85f4c7ad32051742a569fdc1db638af;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;245;-1634.448,411.5589;Inherit;True;Property;_NoiseTex02;NoiseTex02;8;0;Create;False;0;0;0;False;0;False;-1;None;81aae6dfa3129c8459660c6b64b6432e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;226;-1960.439,-1955.966;Inherit;False;0;228;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;222;-895.0446,-1653.489;Inherit;False;Property;_NoiseScale2;扰动_mask;19;0;Create;False;0;0;0;False;0;False;0.1153234;0.1153234;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;207;534.9124,-1378.363;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;244;-1926.629,452.5891;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;239;-1127.201,305.6154;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;227;-1376.006,-1877.115;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;242;-2511.062,373.7379;Inherit;False;0;245;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;191;844.5566,1416.108;Inherit;False;Property;_diss;diss;21;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;213;84.75386,1131.082;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;193;-299.0534,-65.95431;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;243;-2199.516,581.9278;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;202;627.6473,-187.0211;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;199;463.7285,115.1308;Inherit;False;Property;_AddColor;AddColor;3;1;[HDR];Create;True;0;0;0;False;0;False;42.06205,111.9069,137.187,1;1.902029,0.9852941,2,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;215;-294.7284,976.6628;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;223;-495.3205,-2014.91;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;171;-1747.598,60.58946;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;214;-591.8544,973.0988;Inherit;True;Property;_TextureSample1;Texture Sample 1;24;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;224;-1926.525,-1747.939;Inherit;False;Property;_mask_noise_speed;mask_noise_speed;16;0;Create;False;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;160;-2025.23,59.42726;Inherit;False;Property;_Te_Speed_02;Te_Speed_02;12;0;Create;True;0;0;0;False;0;False;0.17,0;0,0.65;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;150;2528.282,-380.7837;Float;False;True;-1;2;ASEMaterialInspector;100;1;XuanFu/Particles/UV_liudong_11;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;155;0;154;1
WireConnection;155;1;154;2
WireConnection;161;0;159;1
WireConnection;161;1;159;2
WireConnection;168;0;164;1
WireConnection;168;1;164;2
WireConnection;172;0;163;0
WireConnection;172;2;155;0
WireConnection;174;0;165;0
WireConnection;174;2;161;0
WireConnection;179;0;175;0
WireConnection;179;2;168;0
WireConnection;170;0;162;1
WireConnection;170;1;162;2
WireConnection;176;1;172;0
WireConnection;180;0;167;1
WireConnection;180;1;167;2
WireConnection;180;2;166;0
WireConnection;177;1;174;0
WireConnection;183;0;179;0
WireConnection;183;1;176;0
WireConnection;183;2;156;0
WireConnection;184;0;180;0
WireConnection;182;0;169;0
WireConnection;182;2;170;0
WireConnection;235;0;237;3
WireConnection;235;1;237;4
WireConnection;186;0;182;0
WireConnection;186;1;177;1
WireConnection;186;2;178;0
WireConnection;190;0;184;0
WireConnection;190;1;188;0
WireConnection;192;1;183;0
WireConnection;195;0;190;0
WireConnection;195;1;192;1
WireConnection;200;0;250;3
WireConnection;200;1;194;0
WireConnection;232;0;234;0
WireConnection;232;1;235;0
WireConnection;189;1;186;0
WireConnection;201;1;232;0
WireConnection;204;0;195;0
WireConnection;204;1;200;0
WireConnection;204;2;198;0
WireConnection;249;0;189;0
WireConnection;249;1;248;0
WireConnection;209;0;201;4
WireConnection;209;1;249;0
WireConnection;208;0;203;4
WireConnection;206;0;204;0
WireConnection;206;2;205;0
WireConnection;247;0;201;0
WireConnection;247;1;246;0
WireConnection;210;0;208;0
WireConnection;212;0;247;0
WireConnection;212;1;209;0
WireConnection;211;0;206;0
WireConnection;181;0;173;0
WireConnection;181;2;171;0
WireConnection;225;0;224;1
WireConnection;225;1;224;2
WireConnection;196;1;193;0
WireConnection;185;1;239;0
WireConnection;216;0;212;0
WireConnection;216;1;210;0
WireConnection;216;2;211;0
WireConnection;228;1;227;0
WireConnection;245;1;244;0
WireConnection;207;0;201;0
WireConnection;244;0;242;0
WireConnection;244;2;243;0
WireConnection;239;0;181;0
WireConnection;239;1;245;1
WireConnection;239;2;240;0
WireConnection;227;0;226;0
WireConnection;227;2;225;0
WireConnection;213;0;184;0
WireConnection;193;0;185;1
WireConnection;193;1;187;0
WireConnection;243;0;241;1
WireConnection;243;1;241;2
WireConnection;202;0;196;0
WireConnection;202;1;199;0
WireConnection;223;0;232;0
WireConnection;223;1;228;1
WireConnection;223;2;222;0
WireConnection;171;0;160;1
WireConnection;171;1;160;2
WireConnection;150;0;216;0
ASEEND*/
//CHKSM=593FC35BC569984DF0CBF1BB0DFA70FCB1D121CD