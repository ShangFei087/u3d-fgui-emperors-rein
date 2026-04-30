// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VertexColor_Alpha"
{
	Properties
	{
		[HDR]_MainColor("MainColor ", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_MainSpeed("MainSpeed", Vector) = (0,0,0,0)
		_NoiseIntensity1("NoiseIntensity1", Range( 0 , 1)) = 0
		_NoiseTex1("NoiseTex1", 2D) = "white" {}
		_NoiseSpeed1("NoiseSpeed1", Vector) = (0,0,0,0)
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_DissolvSpeed("DissolvSpeed", Vector) = (0,0,0,0)
		_Dissolve_ry("Dissolve_ry", Range( 0.51 , 1)) = 0.51
		_NoiseIntensity2("NoiseIntensity2", Range( 0 , 1)) = 0
		_NoiseTex2("NoiseTex2", 2D) = "white" {}
		_NoiseSpeed2("NoiseSpeed2", Vector) = (0,0,0,0)
		_MaskTex("MaskTex", 2D) = "white" {}
		_MaskTex1("MaskTex1", 2D) = "white" {}
		_MaskSpeed("MaskSpeed", Vector) = (0,0,0,0)
		_MaskSpeed1("MaskSpeed1", Vector) = (0,0,0,0)
		_RampTex("RampTex", 2D) = "white" {}
		_RampSpeed("RampSpeed", Vector) = (0,0,0,0)
		[Toggle]_ToggleSwitch1("极坐标开关_溶解", Float) = 0
		[Toggle]_ToggleSwitch0("极坐标开关_主纹理", Float) = 0
		_Float4("极坐标中心X位置", Float) = 0.5
		_Float5("极坐标中心Y位置", Float) = 0.5
		_Float6("极坐标X重铺", Float) = 0
		_Float7("极坐标Y重铺", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("剔除模式", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("深度函数", Float) = 0
		[Enum(on,0,off,1)]_Toggle("深度开关", Float) = 0

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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float _CullMode;
			uniform float _Dst;
			uniform float _Toggle;
			uniform float _Src;
			uniform float _ZTestMode;
			uniform sampler2D _MainTex;
			uniform float2 _MainSpeed;
			uniform float _ToggleSwitch0;
			uniform float4 _MainTex_ST;
			uniform float _Float4;
			uniform float _Float5;
			uniform float _Float6;
			uniform float _Float7;
			uniform sampler2D _NoiseTex1;
			uniform float2 _NoiseSpeed1;
			uniform float4 _NoiseTex1_ST;
			uniform float _NoiseIntensity1;
			uniform float4 _MainColor;
			uniform sampler2D _RampTex;
			uniform float2 _RampSpeed;
			uniform float4 _RampTex_ST;
			uniform float _Dissolve_ry;
			uniform sampler2D _DissolveTex;
			uniform float2 _DissolvSpeed;
			uniform float _ToggleSwitch1;
			uniform float4 _DissolveTex_ST;
			uniform sampler2D _NoiseTex2;
			uniform float2 _NoiseSpeed2;
			uniform float4 _NoiseTex2_ST;
			uniform float _NoiseIntensity2;
			uniform sampler2D _MaskTex;
			uniform float2 _MaskSpeed;
			uniform float4 _MaskTex_ST;
			uniform sampler2D _MaskTex1;
			uniform float2 _MaskSpeed1;
			uniform float4 _MaskTex1_ST;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_color = v.color;
				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
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
				float2 appendResult73 = (float2(_Float4 , _Float5));
				float2 CenteredUV15_g3 = ( uv_MainTex - appendResult73 );
				float2 break17_g3 = CenteredUV15_g3;
				float2 appendResult23_g3 = (float2(( length( CenteredUV15_g3 ) * _Float6 * 2.0 ) , ( atan2( break17_g3.x , break17_g3.y ) * ( 1.0 / 6.28318548202515 ) * _Float7 )));
				float2 panner44 = ( 1.0 * _Time.y * _MainSpeed + (( _ToggleSwitch0 )?( appendResult23_g3 ):( uv_MainTex )));
				float2 uv_NoiseTex1 = i.ase_texcoord1.xy * _NoiseTex1_ST.xy + _NoiseTex1_ST.zw;
				float2 panner34 = ( 1.0 * _Time.y * _NoiseSpeed1 + uv_NoiseTex1);
				float2 temp_cast_0 = (tex2D( _NoiseTex1, panner34 ).r).xx;
				float2 lerpResult37 = lerp( panner44 , temp_cast_0 , _NoiseIntensity1);
				float4 tex2DNode1 = tex2D( _MainTex, lerpResult37 );
				float2 uv_RampTex = i.ase_texcoord1.xy * _RampTex_ST.xy + _RampTex_ST.zw;
				float2 panner82 = ( 1.0 * _Time.y * _RampSpeed + uv_RampTex);
				float2 uv_DissolveTex = i.ase_texcoord1.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
				float2 CenteredUV15_g2 = ( uv_DissolveTex - appendResult73 );
				float2 break17_g2 = CenteredUV15_g2;
				float2 appendResult23_g2 = (float2(( length( CenteredUV15_g2 ) * _Float6 * 2.0 ) , ( atan2( break17_g2.x , break17_g2.y ) * ( 1.0 / 6.28318548202515 ) * _Float7 )));
				float2 panner65 = ( 1.0 * _Time.y * _DissolvSpeed + (( _ToggleSwitch1 )?( appendResult23_g2 ):( uv_DissolveTex )));
				float2 uv_NoiseTex2 = i.ase_texcoord1.xy * _NoiseTex2_ST.xy + _NoiseTex2_ST.zw;
				float2 panner61 = ( 1.0 * _Time.y * _NoiseSpeed2 + uv_NoiseTex2);
				float2 temp_cast_1 = (tex2D( _NoiseTex2, panner61 ).r).xx;
				float2 lerpResult66 = lerp( panner65 , temp_cast_1 , _NoiseIntensity2);
				float smoothstepResult50 = smoothstep( ( 1.0 - _Dissolve_ry ) , _Dissolve_ry , ( tex2D( _DissolveTex, lerpResult66 ).r + 1.0 + ( ( 1.0 - i.ase_color.a ) * -2.0 ) ));
				float2 uv_MaskTex = i.ase_texcoord1.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				float2 panner29 = ( 1.0 * _Time.y * _MaskSpeed + uv_MaskTex);
				float2 uv_MaskTex1 = i.ase_texcoord1.xy * _MaskTex1_ST.xy + _MaskTex1_ST.zw;
				float2 panner87 = ( 1.0 * _Time.y * _MaskSpeed1 + uv_MaskTex1);
				float4 appendResult79 = (float4((( i.ase_color * tex2DNode1 * _MainColor * tex2D( _RampTex, panner82 ) )).rgb , ( smoothstepResult50 * tex2D( _MaskTex, panner29 ).r * _MainColor.a * i.ase_color.a * tex2DNode1.a * tex2D( _MaskTex1, panner87 ).r )));
				
				
				finalColor = appendResult79;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
2285.6;196.8;1905.6;865.4;2533.183;-601.4888;1.842683;True;False
Node;AmplifyShaderEditor.RangedFloatNode;70;-2927.339,49.47449;Inherit;False;Property;_Float5;极坐标中心Y位置;21;0;Create;False;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2920.6,-16.33392;Inherit;False;Property;_Float4;极坐标中心X位置;20;0;Create;False;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2833.49,253.5344;Inherit;False;Property;_Float7;极坐标Y重铺;23;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;41;-2815.782,557.5762;Inherit;False;0;10;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;71;-2836.249,164.0768;Inherit;False;Property;_Float6;极坐标X重铺;22;0;Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;73;-2654.681,14.34897;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;58;-2300.287,365.6051;Inherit;False;0;63;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-2849.965,-161.6584;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;59;-2285.936,516.4183;Inherit;False;Property;_NoiseSpeed2;NoiseSpeed2;11;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;80;-2449.975,688.8721;Inherit;False;Polar Coordinates;-1;;2;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;33;-2551.006,-438.6834;Inherit;False;0;32;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;81;-2154.106,645.6718;Inherit;False;Property;_ToggleSwitch1;极坐标开关_溶解;18;0;Create;False;0;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;43;-2149.725,777.4086;Inherit;False;Property;_DissolvSpeed;DissolvSpeed;7;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;35;-2538.549,-287.8698;Inherit;False;Property;_NoiseSpeed1;NoiseSpeed1;5;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;68;-2471.454,-20.74259;Inherit;False;Polar Coordinates;-1;;3;7dab8e02884cf104ebefaa2e788e4162;0;4;1;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;3;FLOAT;1;False;4;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;61;-2031.464,376.4446;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;65;-1930.769,672.1104;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;63;-1819.762,345.6901;Inherit;True;Property;_NoiseTex2;NoiseTex2;10;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;76;-2166.764,-160.8054;Inherit;False;Property;_ToggleSwitch0;极坐标开关_主纹理;19;0;Create;False;0;0;0;False;0;False;0;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;3;-1144.657,-444.2613;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;45;-2149.931,12.22568;Inherit;False;Property;_MainSpeed;MainSpeed;2;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;64;-1746.717,758.4336;Inherit;False;Property;_NoiseIntensity2;NoiseIntensity2;9;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;34;-2284.075,-427.8439;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;17;-830.4257,360.2771;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-2184.713,247.6869;Inherit;False;Property;_NoiseIntensity1;NoiseIntensity1;3;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;66;-1318.996,486.6682;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;32;-2095.219,-453.7633;Inherit;True;Property;_NoiseTex1;NoiseTex1;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;83;-1557.31,-743.1576;Inherit;False;Property;_RampSpeed;RampSpeed;17;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;84;-1605.135,-910.1722;Inherit;False;0;67;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;44;-1918.883,-142.4349;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-956.8395,847.5794;Inherit;False;Constant;_Float1;Float 1;10;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-943.8835,768.8258;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;31;-1399.625,1263.813;Inherit;False;Property;_MaskSpeed;MaskSpeed;14;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;37;-1592.647,-155.3914;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;10;-1093.656,479.7556;Inherit;True;Property;_DissolveTex;DissolveTex;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;51;-1086.787,949.0056;Inherit;False;Property;_Dissolve_ry;Dissolve_ry;8;0;Create;True;0;0;0;False;0;False;0.51;0;0.51;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-658.4713,564.4097;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;86;-1406.02,1656.595;Inherit;False;Property;_MaskSpeed1;MaskSpeed1;15;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;82;-1292.645,-862.1406;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;85;-1406.192,1513.181;Inherit;False;0;88;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-1399.797,1120.399;Inherit;False;0;18;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;40;-1179.095,-638.3478;Inherit;False;Property;_MainColor;MainColor ;0;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-1272.001,-176.8329;Inherit;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-455.2037,519.7437;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;29;-1124.573,1127.134;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;87;-1130.968,1519.916;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;67;-1005.584,-887.6694;Inherit;True;Property;_RampTex;RampTex;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;52;-716.2932,947.8667;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-914.8956,1100.562;Inherit;True;Property;_MaskTex;MaskTex;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-99.0517,-167.0208;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;88;-923.9664,1493.344;Inherit;True;Property;_MaskTex1;MaskTex1;13;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;50;-269.7873,530.7402;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;162.6441,464.5774;Inherit;True;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;78;118.2099,-173.4716;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-2422.648,902.434;Inherit;False;Property;_CullMode;剔除模式;24;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-2414.038,1260.502;Inherit;False;Property;_Toggle;深度开关;28;1;[Enum];Create;False;0;2;on;0;off;1;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-2418.337,1074.702;Inherit;False;Property;_Dst;Dst;26;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-2423.337,992.002;Inherit;False;Property;_Src;Src;25;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-2418.439,1162.402;Inherit;False;Property;_ZTestMode;深度函数;27;1;[Enum];Create;False;0;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;400.3498,-177.4736;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;626.8373,-176.6126;Float;False;True;-1;2;ASEMaterialInspector;100;1;VertexColor_Alpha;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;1;5;True;54;10;True;55;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;2;True;53;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;True;57;True;3;True;56;True;True;0;False;-1;0;False;-1;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;False;0
WireConnection;73;0;69;0
WireConnection;73;1;70;0
WireConnection;80;1;41;0
WireConnection;80;2;73;0
WireConnection;80;3;71;0
WireConnection;80;4;72;0
WireConnection;81;0;41;0
WireConnection;81;1;80;0
WireConnection;68;1;38;0
WireConnection;68;2;73;0
WireConnection;68;3;71;0
WireConnection;68;4;72;0
WireConnection;61;0;58;0
WireConnection;61;2;59;0
WireConnection;65;0;81;0
WireConnection;65;2;43;0
WireConnection;63;1;61;0
WireConnection;76;0;38;0
WireConnection;76;1;68;0
WireConnection;34;0;33;0
WireConnection;34;2;35;0
WireConnection;17;0;3;4
WireConnection;66;0;65;0
WireConnection;66;1;63;1
WireConnection;66;2;64;0
WireConnection;32;1;34;0
WireConnection;44;0;76;0
WireConnection;44;2;45;0
WireConnection;37;0;44;0
WireConnection;37;1;32;1
WireConnection;37;2;39;0
WireConnection;10;1;66;0
WireConnection;49;0;17;0
WireConnection;49;1;47;0
WireConnection;82;0;84;0
WireConnection;82;2;83;0
WireConnection;1;1;37;0
WireConnection;48;0;10;1
WireConnection;48;1;46;0
WireConnection;48;2;49;0
WireConnection;29;0;28;0
WireConnection;29;2;31;0
WireConnection;87;0;85;0
WireConnection;87;2;86;0
WireConnection;67;1;82;0
WireConnection;52;0;51;0
WireConnection;18;1;29;0
WireConnection;2;0;3;0
WireConnection;2;1;1;0
WireConnection;2;2;40;0
WireConnection;2;3;67;0
WireConnection;88;1;87;0
WireConnection;50;0;48;0
WireConnection;50;1;52;0
WireConnection;50;2;51;0
WireConnection;13;0;50;0
WireConnection;13;1;18;1
WireConnection;13;2;40;4
WireConnection;13;3;3;4
WireConnection;13;4;1;4
WireConnection;13;5;88;1
WireConnection;78;0;2;0
WireConnection;79;0;78;0
WireConnection;79;3;13;0
WireConnection;0;0;79;0
ASEEND*/
//CHKSM=0F8267311D7A5479AA8BBCC86246B60F6513816C