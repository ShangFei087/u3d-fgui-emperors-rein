// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "XuanFu/Particles/More"
{
	Properties
	{
		[Enum(blend,10,add,1)]_Float2("材质模式", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 1
		_MainColor("MainColor", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		[Enum(on,0,off,1)]_Int1("自定义主图偏移", Int) = 1
		_MainTex_UVSpeed("MainTex_UVSpeed", Vector) = (0,0,0,0)
		_MaskTex_01("MaskTex_01", 2D) = "white" {}
		_MaskTex01_UVSpeed("MaskTex01_UVSpeed", Vector) = (0,0,0,0)
		_MaskTex_02("MaskTex_02", 2D) = "white" {}
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_Noise_Scale("Noise_Scale", Range( 0 , 0.5)) = 0
		_NoiseTex_UVSpeed("NoiseTex_UVSpeed", Vector) = (0,0,0,0)
		[Header(___________________________________________________________________________________________________________________________________________________________________________________________________________________________________________)][Header(Dissolove)]_dissolvetex1("溶解贴图", 2D) = "white" {}
		_Float7("溶解", Range( 0 , 1)) = 0
		_Float9("软硬", Range( 0.5 , 1)) = 0.5
		[Toggle]_Float12("custom2x控制溶解", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha [_Float2]
		AlphaToMask Off
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		
		
		
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
				float4 ase_texcoord2 : TEXCOORD2;
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
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _Float2;
			uniform float _CullMode;
			uniform float4 _MainColor;
			uniform sampler2D _MainTex;
			uniform float2 _MainTex_UVSpeed;
			uniform int _Int1;
			uniform float4 _MainTex_ST;
			uniform sampler2D _NoiseTex;
			uniform float2 _NoiseTex_UVSpeed;
			uniform float4 _NoiseTex_ST;
			uniform float _Noise_Scale;
			uniform sampler2D _MaskTex_01;
			uniform float2 _MaskTex01_UVSpeed;
			uniform float4 _MaskTex_01_ST;
			uniform sampler2D _MaskTex_02;
			uniform float4 _MaskTex_02_ST;
			uniform float _Float9;
			uniform sampler2D _dissolvetex1;
			uniform float4 _dissolvetex1_ST;
			uniform float _Float7;
			uniform float _Float12;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				o.ase_texcoord3 = v.ase_texcoord2;
				
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
				float4 texCoord67 = i.ase_texcoord1;
				texCoord67.xy = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult69 = (float2(texCoord67.x , texCoord67.y));
				float2 appendResult11 = (float2(_MainTex_UVSpeed.x , _MainTex_UVSpeed.y));
				float2 lerpResult68 = lerp( appendResult69 , appendResult11 , (float)_Int1);
				float2 uv_MainTex = i.ase_texcoord2.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 panner7 = ( 1.0 * _Time.y * lerpResult68 + uv_MainTex);
				float2 appendResult18 = (float2(_NoiseTex_UVSpeed.x , _NoiseTex_UVSpeed.y));
				float2 uv_NoiseTex = i.ase_texcoord2.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				float2 panner20 = ( 1.0 * _Time.y * appendResult18 + uv_NoiseTex);
				float4 tex2DNode8 = tex2D( _NoiseTex, panner20 );
				float2 temp_cast_1 = (tex2DNode8.r).xx;
				float2 lerpResult23 = lerp( panner7 , temp_cast_1 , _Noise_Scale);
				float4 tex2DNode5 = tex2D( _MainTex, lerpResult23 );
				float2 appendResult37 = (float2(_MaskTex01_UVSpeed.x , _MaskTex01_UVSpeed.y));
				float2 uv_MaskTex_01 = i.ase_texcoord2.xy * _MaskTex_01_ST.xy + _MaskTex_01_ST.zw;
				float2 panner38 = ( 1.0 * _Time.y * appendResult37 + uv_MaskTex_01);
				float2 uv_MaskTex_02 = i.ase_texcoord2.xy * _MaskTex_02_ST.xy + _MaskTex_02_ST.zw;
				float2 temp_cast_2 = (tex2DNode8.r).xx;
				float2 lerpResult63 = lerp( uv_MaskTex_02 , temp_cast_2 , _Noise_Scale);
				float2 uv_dissolvetex1 = i.ase_texcoord2.xy * _dissolvetex1_ST.xy + _dissolvetex1_ST.zw;
				float4 texCoord99 = i.ase_texcoord3;
				texCoord99.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float lerpResult85 = lerp( _Float7 , texCoord99.x , _Float12);
				float smoothstepResult97 = smoothstep( ( 1.0 - _Float9 ) , _Float9 , saturate( ( ( tex2D( _dissolvetex1, uv_dissolvetex1 ).r + 1.0 ) - ( lerpResult85 * 2.0 ) ) ));
				float4 appendResult26 = (float4((( i.ase_color * _MainColor * tex2DNode5 )).rgb , ( tex2DNode5.a * ( tex2D( _MaskTex_01, panner38 ).r * tex2D( _MaskTex_02, lerpResult63 ).r ) * i.ase_color.a * smoothstepResult97 * _MainColor.a )));
				
				
				finalColor = appendResult26;
				return finalColor;
			}
			ENDCG
		}
	}
	
}
/*ASEBEGIN
Version=18900
2910;219;2274;1111;-599.98;1051.047;1.3;True;True
Node;AmplifyShaderEditor.Vector2Node;70;-1225.454,-510.4661;Inherit;False;Property;_MainTex_UVSpeed;MainTex_UVSpeed;5;0;Create;True;0;0;0;False;0;False;0,0;-0.69,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.CommentaryNode;73;-1013.953,740.6042;Inherit;False;2876.65;1092.237;软溶解;14;103;100;99;97;95;94;93;92;91;90;87;86;85;84;软溶解;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;22;-2462.668,237.6544;Inherit;False;Property;_NoiseTex_UVSpeed;NoiseTex_UVSpeed;11;0;Create;True;0;0;0;False;0;False;0,0;0.26,0.06;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;67;-1174.547,-319.4128;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;100;-885.9503,1050.686;Inherit;False;Property;_Float7;溶解;13;0;Create;False;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;72;-1125.368,-108.4197;Inherit;False;Property;_Int1;自定义主图偏移;4;1;[Enum];Create;False;0;2;on;0;off;1;0;False;0;False;1;1;False;0;1;INT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;99;-822.2182,1172.195;Inherit;True;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;103;-542.8494,1281.866;Inherit;False;Property;_Float12;custom2x控制溶解;15;1;[Toggle];Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-793.2634,-320.6271;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-2164.838,231.8924;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;11;-839.3361,-512.4489;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;21;-2475.194,97.58089;Inherit;False;0;8;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;85;-50.59016,1093.679;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;748.4346,1382.118;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;548.505,1364.351;Inherit;False;Constant;_Float3;Float 3;11;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;40;-728.0778,281.8909;Inherit;False;Property;_MaskTex01_UVSpeed;MaskTex01_UVSpeed;7;0;Create;True;0;0;0;False;0;False;0,0;-0.69,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;84;155.361,854.2183;Inherit;True;Property;_dissolvetex1;溶解贴图;12;0;Create;False;0;0;0;False;2;Header(___________________________________________________________________________________________________________________________________________________________________________________________________________________________________________);Header(Dissolove);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;68;-523.6932,-335.1985;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1213.333,-712.3674;Inherit;False;0;5;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;20;-1903.899,102.7947;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;8;-1631.667,93.97694;Inherit;True;Property;_NoiseTex;NoiseTex;9;0;Create;True;0;0;0;False;0;False;-1;None;712a3df1a9190f64b8105b3c345ae003;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;970.4987,1180.154;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;37;-430.2472,276.1287;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;62;-713.6029,481.5284;Inherit;False;0;34;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-1603.046,316.1052;Inherit;False;Property;_Noise_Scale;Noise_Scale;10;0;Create;True;0;0;0;False;0;False;0;0.048;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;874.5027,1012.972;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-740.6028,141.8182;Inherit;False;0;27;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;7;-216.0513,-541.4734;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;63;-90.75583,522.8332;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;92;1006.363,1037.411;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;38;-169.3079,147.0321;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;23;106.0853,-440.8979;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;93;902.9461,1420.574;Inherit;False;Property;_Float9;软硬;14;0;Create;False;0;0;0;False;0;False;0.5;0.5;0.5;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;43;490.3422,-716.395;Inherit;False;Property;_MainColor;MainColor;2;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;44;447.0269,-260.0168;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;27;199.2011,137.395;Inherit;True;Property;_MaskTex_01;MaskTex_01;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;94;1228.488,1009.644;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;95;1140.386,1147.137;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;34;200.6599,362.0591;Inherit;True;Property;_MaskTex_02;MaskTex_02;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;462.5161,-509.774;Inherit;True;Property;_MainTex;MainTex;3;0;Create;True;0;0;0;False;0;False;-1;None;99eead84a91fb8948925aa47724d71e5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;705.4245,302.2459;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;97;1392.527,1063.901;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;1008.078,-459.0905;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;1445.75,220.9284;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;25;1481.303,-487.3803;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;105;2457.445,-206.0839;Inherit;False;Property;_Float2;材质模式;0;1;[Enum];Create;False;0;2;blend;10;add;1;0;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;2456.369,-295.2739;Inherit;False;Property;_CullMode;剔除模式;1;1;[Enum];Create;False;0;1;Option1;0;1;UnityEngine.Rendering.CullMode;True;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;26;1907.534,-231.1505;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2166.174,-229.2838;Float;False;True;-1;2;ASEMaterialInspector;100;1;XuanFu/Particles/More;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;1;5;False;104;1;True;105;0;0;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;3;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;False;-1;True;3;False;-1;True;False;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;69;0;67;1
WireConnection;69;1;67;2
WireConnection;18;0;22;1
WireConnection;18;1;22;2
WireConnection;11;0;70;1
WireConnection;11;1;70;2
WireConnection;85;0;100;0
WireConnection;85;1;99;1
WireConnection;85;2;103;0
WireConnection;68;0;69;0
WireConnection;68;1;11;0
WireConnection;68;2;72;0
WireConnection;20;0;21;0
WireConnection;20;2;18;0
WireConnection;8;1;20;0
WireConnection;91;0;85;0
WireConnection;91;1;86;0
WireConnection;37;0;40;1
WireConnection;37;1;40;2
WireConnection;90;0;84;1
WireConnection;90;1;87;0
WireConnection;7;0;6;0
WireConnection;7;2;68;0
WireConnection;63;0;62;0
WireConnection;63;1;8;1
WireConnection;63;2;24;0
WireConnection;92;0;90;0
WireConnection;92;1;91;0
WireConnection;38;0;39;0
WireConnection;38;2;37;0
WireConnection;23;0;7;0
WireConnection;23;1;8;1
WireConnection;23;2;24;0
WireConnection;27;1;38;0
WireConnection;94;0;92;0
WireConnection;95;0;93;0
WireConnection;34;1;63;0
WireConnection;5;1;23;0
WireConnection;35;0;27;1
WireConnection;35;1;34;1
WireConnection;97;0;94;0
WireConnection;97;1;95;0
WireConnection;97;2;93;0
WireConnection;41;0;44;0
WireConnection;41;1;43;0
WireConnection;41;2;5;0
WireConnection;36;0;5;4
WireConnection;36;1;35;0
WireConnection;36;2;44;4
WireConnection;36;3;97;0
WireConnection;36;4;43;4
WireConnection;25;0;41;0
WireConnection;26;0;25;0
WireConnection;26;3;36;0
WireConnection;0;0;26;0
ASEEND*/
//CHKSM=CE1BCA2CC38CE300FF7CB25D486A6F74373161F7