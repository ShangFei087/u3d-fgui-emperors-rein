// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "custom_sexiang"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_MainColor("MainColor", Color) = (1,1,1,1)
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_DissolveSpeed("DissolveSpeed", Vector) = (0,0,0,0)
		_MaskTex("MaskTex", 2D) = "white" {}
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_NoiseSpeed("NoiseSpeed", Vector) = (0,0,0,0)
		_Dissolve_ry("Dissolve_ry", Range( 0.51 , 1)) = 0.51
		_intensity("强度", Range( 0 , 2)) = 1
		_RampTex("RampTex", 2D) = "white" {}
		_Speed("Speed", Float) = 1
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 10
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
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord : TEXCOORD0;
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

			uniform float _ZTestMode;
			uniform float _CullMode;
			uniform float _Src;
			uniform float _Dst;
			uniform float _Toggle;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform sampler2D _NoiseTex;
			uniform float2 _NoiseSpeed;
			uniform float4 _NoiseTex_ST;
			uniform float4 _MainColor;
			uniform sampler2D _RampTex;
			uniform float4 _RampTex_ST;
			uniform float _Speed;
			uniform sampler2D _MaskTex;
			uniform float4 _MaskTex_ST;
			uniform float _intensity;
			uniform float _Dissolve_ry;
			uniform sampler2D _DissolveTex;
			uniform float2 _DissolveSpeed;
			uniform float4 _DissolveTex_ST;
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
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
				float4 texCoord7 = i.ase_texcoord1;
				texCoord7.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult8 = (float2(texCoord7.x , texCoord7.y));
				float2 uv_MainTex = i.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 panner35 = ( 1.0 * _Time.y * appendResult8 + uv_MainTex);
				float2 appendResult30 = (float2(_NoiseSpeed.x , _NoiseSpeed.y));
				float2 uv_NoiseTex = i.ase_texcoord2.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 panner28 = ( 1.0 * _Time.y * appendResult30 + uv_NoiseTex);
				float2 temp_cast_0 = (tex2D( _NoiseTex, panner28 ).r).xx;
				float2 lerpResult9 = lerp( panner35 , temp_cast_0 , texCoord7.z);
				float4 tex2DNode1 = tex2D( _MainTex, lerpResult9 );
				float2 uv_RampTex = i.ase_texcoord2.xy * _RampTex_ST.xy + _RampTex_ST.zw;
				float3 hsvTorgb79 = RGBToHSV( ( i.ase_color * tex2DNode1 * _MainColor * tex2D( _RampTex, uv_RampTex ) ).rgb );
				float mulTime81 = _Time.y * _Speed;
				float3 hsvTorgb83 = HSVToRGB( float3(( hsvTorgb79 + mulTime81 ).x,hsvTorgb79.y,hsvTorgb79.z) );
				float2 uv_MaskTex = i.ase_texcoord2.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				float2 panner26 = ( 1.0 * _Time.y * float2( 0,0 ) + uv_MaskTex);
				float2 texCoord50 = i.ase_texcoord2.xy * float2( 1,1 ) + float2( 0,0 );
				float temp_output_52_0 = distance( float2( 0.5,0.5 ) , texCoord50 );
				float smoothstepResult57 = smoothstep( ( texCoord7.w - 1.0 ) , 1.0 , temp_output_52_0);
				float smoothstepResult54 = smoothstep( ( 1.0 - texCoord7.w ) , _intensity , ( 1.0 - temp_output_52_0 ));
				float smoothstepResult62 = smoothstep( 0.0 , 0.5 , ( smoothstepResult57 * smoothstepResult54 ));
				float2 uv_DissolveTex = i.ase_texcoord2.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float2 panner33 = ( 1.0 * _Time.y * _DissolveSpeed + uv_DissolveTex);
				float smoothstepResult42 = smoothstep( ( 1.0 - _Dissolve_ry ) , _Dissolve_ry , ( tex2D( _DissolveTex, panner33 ).r + 1.0 + ( ( 1.0 - i.ase_color.a ) * -2.0 ) ));
				float4 appendResult78 = (float4((hsvTorgb83).xyz , ( ( tex2D( _MaskTex, panner26 ).r * smoothstepResult62 ) * smoothstepResult42 * _MainColor.a * i.ase_color.a * tex2DNode1.a )));
				
				
				finalColor = appendResult78;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
2068;61.6;1910.4;998.2;3870.955;748.056;2.10433;True;False
Node;AmplifyShaderEditor.Vector2Node;29;-2815.339,415.8203;Inherit;False;Property;_NoiseSpeed;NoiseSpeed;6;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;22;-2860.948,271.8068;Inherit;False;0;6;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;30;-2596.562,426.9297;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2847.277,50.28529;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;28;-2450.745,283.5905;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-2838.763,-170.9411;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;8;-2538.687,15.94158;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;6;-2267.512,249.5262;Inherit;True;Property;_NoiseTex;NoiseTex;5;0;Create;True;0;0;0;False;0;False;-1;None;202ee2239c40f9147ba16afe53caffe5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;50;-2405.332,1475.728;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;51;-2379.72,1354.339;Inherit;False;Constant;_Vector0;Vector 0;9;0;Create;True;0;0;0;False;0;False;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;35;-2326.898,-55.77093;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-2247.573,1256.998;Inherit;False;Constant;_Float3;Float 3;11;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;52;-2138.517,1402.263;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-1986.07,-64.88805;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;32;-1787.811,464.0482;Inherit;False;0;11;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;34;-1750.268,593.147;Inherit;False;Property;_DissolveSpeed;DissolveSpeed;3;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;1;-1702.159,-46.77966;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;4232c6a4d9ae0f5448be2cf45f2aa958;4232c6a4d9ae0f5448be2cf45f2aa958;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;56;-2120.158,1771.871;Inherit;True;Property;_intensity;强度;8;0;Create;False;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;16;-1450.875,-340.873;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;53;-1879.73,1441.095;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;59;-1869.666,1257.214;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;67;-1580.944,-762.0291;Inherit;True;Property;_RampTex;RampTex;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;58;-2020.736,1668.719;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-1494.038,-535.5933;Inherit;False;Property;_MainColor;MainColor;1;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;33;-1520.268,468.1467;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-463.1952,15.87134;Inherit;False;Property;_Speed;Speed;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1351.702,741.1804;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;54;-1642.975,1505.278;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-569.966,-231.0303;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-1766.762,984.1482;Inherit;True;0;14;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;66;-1340.679,212.2696;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;57;-1578.563,1261.29;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;81;-312.4585,22.15203;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-1187.05,1726.493;Inherit;False;Constant;_Float4;Float 4;11;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;26;-1486.136,986.3032;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;-1338.145,430.4162;Inherit;True;Property;_DissolveTex;DissolveTex;2;0;Create;True;0;0;0;False;0;False;-1;None;0fd514ad4876b124690bd399ae498852;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-1216.879,628.6788;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;79;-302.5709,-233.2822;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-1234.257,1487.165;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-1309.119,847.8292;Inherit;False;Property;_Dissolve_ry;Dissolve_ry;7;0;Create;True;0;0;0;False;0;False;0.51;0;0.51;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-1164.9,724.5166;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-987.8437,524.7198;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;44;-930.3171,809.1945;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;14;-1044.368,968.512;Inherit;True;Property;_MaskTex;MaskTex;4;0;Create;True;0;0;0;False;0;False;-1;8c4a7fca2884fab419769ccc0355c0c1;26128c5febc677440ac5b3c79d43d692;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;62;-975.513,1494.177;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;82;-2.611403,-46.93567;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-629.1284,998.5881;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;42;-832.1956,500.357;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;83;119.6239,-247.6096;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-452.474,256.8474;Inherit;True;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;77;483.1337,-168.8891;Inherit;False;True;True;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;73;-3066.117,1843.201;Inherit;False;Property;_Dst;Dst;13;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-3064.955,1758.611;Inherit;False;Property;_Src;Src;12;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-3065.779,2033.526;Inherit;False;Property;_Toggle;深度开关;15;1;[Enum];Create;False;0;2;on;0;off;1;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-3063.792,1668.735;Inherit;False;Property;_CullMode;剔除模式;11;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-3066.379,1940.126;Inherit;False;Property;_ZTestMode;深度函数;14;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;78;809.8055,-173.625;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1125.924,-168.9082;Float;False;True;-1;2;ASEMaterialInspector;100;1;custom_sexiang;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;2;5;True;72;10;True;73;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;2;True;71;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;True;75;True;3;True;74;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;30;0;29;1
WireConnection;30;1;29;2
WireConnection;28;0;22;0
WireConnection;28;2;30;0
WireConnection;8;0;7;1
WireConnection;8;1;7;2
WireConnection;6;1;28;0
WireConnection;35;0;4;0
WireConnection;35;2;8;0
WireConnection;52;0;51;0
WireConnection;52;1;50;0
WireConnection;9;0;35;0
WireConnection;9;1;6;1
WireConnection;9;2;7;3
WireConnection;1;1;9;0
WireConnection;53;0;52;0
WireConnection;59;0;7;4
WireConnection;59;1;60;0
WireConnection;58;0;7;4
WireConnection;33;0;32;0
WireConnection;33;2;34;0
WireConnection;54;0;53;0
WireConnection;54;1;58;0
WireConnection;54;2;56;0
WireConnection;17;0;16;0
WireConnection;17;1;1;0
WireConnection;17;2;31;0
WireConnection;17;3;67;0
WireConnection;66;0;16;4
WireConnection;57;0;52;0
WireConnection;57;1;59;0
WireConnection;81;0;80;0
WireConnection;26;0;27;0
WireConnection;11;1;33;0
WireConnection;79;0;17;0
WireConnection;61;0;57;0
WireConnection;61;1;54;0
WireConnection;41;0;66;0
WireConnection;41;1;40;0
WireConnection;39;0;11;1
WireConnection;39;1;38;0
WireConnection;39;2;41;0
WireConnection;44;0;43;0
WireConnection;14;1;26;0
WireConnection;62;0;61;0
WireConnection;62;2;63;0
WireConnection;82;0;79;0
WireConnection;82;1;81;0
WireConnection;65;0;14;1
WireConnection;65;1;62;0
WireConnection;42;0;39;0
WireConnection;42;1;44;0
WireConnection;42;2;43;0
WireConnection;83;0;82;0
WireConnection;83;1;79;2
WireConnection;83;2;79;3
WireConnection;13;0;65;0
WireConnection;13;1;42;0
WireConnection;13;2;31;4
WireConnection;13;3;16;4
WireConnection;13;4;1;4
WireConnection;77;0;83;0
WireConnection;78;0;77;0
WireConnection;78;3;13;0
WireConnection;0;0;78;0
ASEEND*/
//CHKSM=C3E2B12C2B45F93806A3A668564839A4FDDE4A58