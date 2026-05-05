// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "custom_alpha"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_MainUV("MainUV", Vector) = (0,0,0,0)
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_DissolveSpeed("DissolveSpeed", Vector) = (0,0,0,0)
		_MaskTex1("MaskTex1", 2D) = "white" {}
		_MaskSpeed("MaskSpeed", Vector) = (0,0,0,0)
		_MaskTex2("MaskTex2", 2D) = "white" {}
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_NoiseSpeed("NoiseSpeed", Vector) = (0,0,0,0)
		_Dissolve_ry("Dissolve_ry", Range( 0.51 , 1)) = 0.51
		_RampTex("RampTex", 2D) = "white" {}
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("深度函数", Float) = 0
		[Enum(on,0,off,1)]_Toggle("深度开关", Float) = 0
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
				float4 ase_color : COLOR;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Dst;
			uniform float _CullMode;
			uniform float _Src;
			uniform float _Toggle;
			uniform float _ZTestMode;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float2 _MainUV;
			uniform sampler2D _NoiseTex;
			uniform float2 _NoiseSpeed;
			uniform float4 _NoiseTex_ST;
			uniform float4 _MainColor;
			uniform sampler2D _RampTex;
			uniform float4 _RampTex_ST;
			uniform float _Dissolve_ry;
			uniform sampler2D _DissolveTex;
			uniform float2 _DissolveSpeed;
			uniform float4 _DissolveTex_ST;
			uniform sampler2D _MaskTex1;
			uniform float2 _MaskSpeed;
			uniform float4 _MaskTex1_ST;
			uniform sampler2D _MaskTex2;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_texcoord2 = v.ase_texcoord1;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
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
				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 appendResult48 = (float2(_MainUV.x , _MainUV.y));
				float4 texCoord7 = i.ase_texcoord2;
				texCoord7.xy = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult8 = (float2(texCoord7.x , texCoord7.y));
				float2 panner35 = ( 1.0 * _Time.y * float2( 0,0 ) + (uv_MainTex*appendResult48 + appendResult8));
				float2 appendResult30 = (float2(_NoiseSpeed.x , _NoiseSpeed.y));
				float2 uv_NoiseTex = i.ase_texcoord1.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 panner28 = ( 1.0 * _Time.y * appendResult30 + uv_NoiseTex);
				float2 temp_cast_0 = (tex2D( _NoiseTex, panner28 ).r).xx;
				float2 lerpResult9 = lerp( panner35 , temp_cast_0 , texCoord7.z);
				float4 tex2DNode1 = tex2D( _MainTex, lerpResult9 );
				float2 uv_RampTex = i.ase_texcoord1.xy * _RampTex_ST.xy + _RampTex_ST.zw;
				float2 uv_DissolveTex = i.ase_texcoord1.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float2 panner33 = ( 1.0 * _Time.y * _DissolveSpeed + uv_DissolveTex);
				float smoothstepResult42 = smoothstep( ( 1.0 - _Dissolve_ry ) , _Dissolve_ry , ( tex2D( _DissolveTex, panner33 ).r + 1.0 + ( texCoord7.w * -2.0 ) ));
				float2 uv_MaskTex1 = i.ase_texcoord1.xy * _MaskTex1_ST.xy + _MaskTex1_ST.zw;
				float2 panner26 = ( 1.0 * _Time.y * _MaskSpeed + uv_MaskTex1);
				float2 panner56 = ( 1.0 * _Time.y * float2( 0,0 ) + uv_MaskTex1);
				float4 appendResult3 = (float4((( i.ase_color * tex2DNode1 * _MainColor * tex2D( _RampTex, uv_RampTex ) )).rgb , ( tex2DNode1.a * smoothstepResult42 * tex2D( _MaskTex1, panner26 ).r * i.ase_color.a * _MainColor.a * tex2D( _MaskTex2, panner56 ).r )));
				
				
				finalColor = appendResult3;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
128.8;82.4;1905.6;870.2;2860.735;-425.4784;1.466553;True;False
Node;AmplifyShaderEditor.Vector2Node;29;-2171.274,404.8298;Inherit;False;Property;_NoiseSpeed;NoiseSpeed;9;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2445.236,39.15643;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;46;-2249.371,-119.2954;Inherit;False;Property;_MainUV;MainUV;2;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;22;-2175.119,263.0144;Inherit;False;0;6;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;30;-1978.874,426.9298;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-2288.112,-246.4248;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;48;-2068.551,-113.2819;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;8;-2161.966,28.53504;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;45;-1832.016,-118.0775;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;1,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-1791.268,476.1467;Inherit;False;0;11;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;28;-1833.057,283.5906;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;34;-1750.268,606.147;Inherit;False;Property;_DissolveSpeed;DissolveSpeed;4;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;35;-1593.626,-116.9648;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;6;-1651.709,228.7914;Inherit;True;Property;_NoiseTex;NoiseTex;8;0;Create;True;0;0;0;False;0;False;-1;None;202ee2239c40f9147ba16afe53caffe5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;40;-1351.702,765.1256;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;33;-1520.268,468.1467;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;-1338.145,430.4162;Inherit;True;Property;_DissolveTex;DissolveTex;3;0;Create;True;0;0;0;False;0;False;-1;None;0fd514ad4876b124690bd399ae498852;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-1164.9,724.5166;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1309.119,847.8292;Inherit;False;Property;_Dissolve_ry;Dissolve_ry;10;0;Create;True;0;0;0;False;0;False;0.51;0;0.51;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;58;-1590.7,1127.371;Inherit;False;Property;_MaskSpeed;MaskSpeed;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;9;-1371.087,-97.33022;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-1635.613,963.1933;Inherit;False;0;14;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-1216.879,628.6788;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;55;-1638.436,1274.856;Inherit;False;0;14;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-987.8437,524.7198;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-1039.526,-388.6119;Inherit;False;Property;_MainColor;MainColor;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;16;-1034.91,-195.9205;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;26;-1356.546,968.5998;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;44;-961.3964,820.6312;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-1088.613,-15.71128;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;4232c6a4d9ae0f5448be2cf45f2aa958;4232c6a4d9ae0f5448be2cf45f2aa958;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;56;-1359.369,1280.263;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;49;-1099.579,-618.4554;Inherit;True;Property;_RampTex;RampTex;11;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1153.576,934.9622;Inherit;True;Property;_MaskTex1;MaskTex1;5;0;Create;True;0;0;0;False;0;False;-1;None;26128c5febc677440ac5b3c79d43d692;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;57;-1156.4,1246.625;Inherit;True;Property;_MaskTex2;MaskTex2;7;0;Create;True;0;0;0;False;0;False;-1;None;26128c5febc677440ac5b3c79d43d692;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;42;-792.6965,519.1385;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-635.2078,-62.77166;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-567.84,206.7788;Inherit;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;2;-459.0283,-35.19693;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2485.657,545.2328;Inherit;False;Property;_CullMode;剔除模式;12;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2471.737,657.6965;Inherit;False;Property;_Src;Src;13;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-2476.66,1002.505;Inherit;False;Property;_Toggle;深度开关;16;1;[Enum];Create;False;0;2;on;0;off;1;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-2482.558,872.0464;Inherit;False;Property;_ZTestMode;深度函数;15;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;3;-187.3366,-40.58723;Inherit;True;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-2476.66,765.662;Inherit;False;Property;_Dst;Dst;14;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;82.13554,-37.02168;Float;False;True;-1;2;ASEMaterialInspector;100;1;custom_alpha;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;1;5;True;51;10;True;52;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;2;True;50;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;True;54;True;3;True;53;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;30;0;29;1
WireConnection;30;1;29;2
WireConnection;48;0;46;1
WireConnection;48;1;46;2
WireConnection;8;0;7;1
WireConnection;8;1;7;2
WireConnection;45;0;4;0
WireConnection;45;1;48;0
WireConnection;45;2;8;0
WireConnection;28;0;22;0
WireConnection;28;2;30;0
WireConnection;35;0;45;0
WireConnection;6;1;28;0
WireConnection;33;0;32;0
WireConnection;33;2;34;0
WireConnection;11;1;33;0
WireConnection;41;0;7;4
WireConnection;41;1;40;0
WireConnection;9;0;35;0
WireConnection;9;1;6;1
WireConnection;9;2;7;3
WireConnection;39;0;11;1
WireConnection;39;1;38;0
WireConnection;39;2;41;0
WireConnection;26;0;27;0
WireConnection;26;2;58;0
WireConnection;44;0;43;0
WireConnection;1;1;9;0
WireConnection;56;0;55;0
WireConnection;14;1;26;0
WireConnection;57;1;56;0
WireConnection;42;0;39;0
WireConnection;42;1;44;0
WireConnection;42;2;43;0
WireConnection;17;0;16;0
WireConnection;17;1;1;0
WireConnection;17;2;31;0
WireConnection;17;3;49;0
WireConnection;13;0;1;4
WireConnection;13;1;42;0
WireConnection;13;2;14;1
WireConnection;13;3;16;4
WireConnection;13;4;31;4
WireConnection;13;5;57;1
WireConnection;2;0;17;0
WireConnection;3;0;2;0
WireConnection;3;3;13;0
WireConnection;0;0;3;0
ASEEND*/
//CHKSM=A7DE89534A1B20156A3E8B7F73BD7ACFC315F4CE