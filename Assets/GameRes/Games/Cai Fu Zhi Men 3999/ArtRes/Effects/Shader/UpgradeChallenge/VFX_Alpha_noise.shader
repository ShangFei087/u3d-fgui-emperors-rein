Shader "S_Game_Effects/Noise_Step_blend"
{
	Properties
	{
		[HDR]_Main_Color("Main_Color", Color) =(1,1,1,1)
		_Main_Tex("Main_Tex", 2D) = "white" {}
		[ScaleOffset] _MainTex_Scroll("Scroll", Vector) =(0, 0, 0, 0)
		_Main_strength("Main_strength", Float) = 1
		[HDR]_Step_edgcolor("Step_edg/color", Color) =(1,1,1,0)
		_NoiseStep_Tex("Noise/Step_Tex", 2D) = "white" {}
		[ScaleOffset] _Noise_Scroll("Scroll", Vector) =(0, 0, 0, 0)
		_Noise_strength("Noise_strength", Range( 0 , 1)) = 0
		_Step_strength("Step_strength", Range( 0 , 1)) = 1
		_Step_Hardness("Step_Hardness", Range( 0 , 1)) = 0.8
		_Stpe_edg_width("Stpe_edg_width", Range( 0 , 1)) = 0
		[KeywordEnum(noise,step,opaque)] _Noise_for_Opaque("Noise_for_Opaque", Float) = 0
		_Mask_Opaque_Tex("Mask_Opaque_Tex", 2D) = "white" {}
		[ScaleOffset] _Mask_Scroll("Scroll", Vector) =(0, 0, 0, 0)
		[Toggle(_MASK_FOR_NOISE_ON)] _Mask_for_noise("Mask_for_noise", Float) = 0
		[Toggle(_PART_UV2_ONOFF_ON)] _Part_uv2_onoff("Part_uv2_on/off", Float) = 0
		[Toggle(_PART_UV3_ONOFF_ON)] _Part_uv3_onoff("Part_uv3_on/off", Float) = 0

		[Header(Render State)]
		[Enum(Off, 0, Front, 1, Back, 2)]_CullMode("Cull Mode", float) = 2
		[Enum(On, 4, Off, 8)] _ZTestMode("ZTest Mode", float) = 4
		[Enum(On, 1, Off, 0)] _ZWriteMode("ZWrite Mode", float) = 1
		[HideInInspector] _texcoord( "", 2D) = "white" {}
	}
	
	SubShader
	{
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		CGINCLUDE
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		Cull [_CullMode]
		ZWrite [_ZWriteMode]
		ZTest [_ZTestMode]		
		
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile _DUMMY _PART_UV2_ONOFF_ON
			#pragma multi_compile _DUMMY _MASK_FOR_NOISE_ON
			#pragma multi_compile _DUMMY _PART_UV3_ONOFF_ON
			#pragma multi_compile _NOISE_FOR_OPAQUE_NOISE _NOISE_FOR_OPAQUE_STEP _NOISE_FOR_OPAQUE_OPAQUE


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0; // Main + 0
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_color : COLOR;
				float4 UV2Tex344 : TEXCOORD3;
				float4 TexUV : TEXCOORD4; // Noise + Mask
			};

			half _ZTest_Mode;
			half _Cull_Mode;
			half4 _Step_edgcolor;
			half4 _Main_Color;
			sampler2D _Main_Tex;
			half4 _Main_Tex_ST;
			half2 _MainTex_Scroll;
			half _Noise_strength;
			half _Step_strength;
			sampler2D _NoiseStep_Tex;
			half4 _NoiseStep_Tex_ST;
			half2 _Noise_Scroll;
			sampler2D _Mask_Opaque_Tex;
			half4 _Mask_Opaque_Tex_ST;
			half2 _Mask_Scroll;
			half _Main_strength;
			half _Stpe_edg_width;
			half _Step_Hardness;
			
			#define TRANSFORM_TEX2(tex1, name1, tex2, name2)(float4(tex1.xy, tex2.xy) * float4(name1##_ST.xy, name2##_ST.xy) + float4(name1##_ST.zw, name2##_ST.zw))

			
			v2f vert( appdata v)
			{
				v2f o;

				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_color = v.color;
				
				o.vertex = UnityObjectToClipPos(v.vertex);

				half2 uv0_Main_Tex = TRANSFORM_TEX(v.ase_texcoord.xy, _Main_Tex);
				o.ase_texcoord.xy = uv0_Main_Tex;
				o.ase_texcoord.zw = 0;

				half4 UV2Tex344 = half4(_Noise_strength, _Step_strength, uv0_Main_Tex);
			#if _PART_UV2_ONOFF_ON
				UV2Tex344 += v.ase_texcoord1;
			#endif
				o.UV2Tex344 = UV2Tex344;

				o.TexUV = TRANSFORM_TEX2(v.ase_texcoord.xy, _NoiseStep_Tex, v.ase_texcoord.xy, _Mask_Opaque_Tex);
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 finalColor;
				half4 UV2Tex344 = i.UV2Tex344;
				UV2Tex344.zw += _Time.y * _MainTex_Scroll;

				half2 noisestep_uv = i.TexUV.xy + _Time.y * _Noise_Scroll;
				half2 maskopaque_uv = i.TexUV.zw;
			#if _PART_UV3_ONOFF_ON
				half4 UV3NoiseMask219 = half4(noisestep_uv, i.ase_texcoord.xy + _Time.y * _Mask_Scroll);
				UV3NoiseMask219 += i.ase_texcoord2;
			#else
				half4 UV3NoiseMask219 = half4(noisestep_uv , maskopaque_uv + _Time.y * _Mask_Scroll);
			#endif
				half4 noise_map_color = tex2D( _NoiseStep_Tex, i.TexUV.xy);
				half4 tex2DNode192 = tex2D( _NoiseStep_Tex, UV3NoiseMask219.xy +( noise_map_color.r - 0.5) * UV2Tex344.x);
				half temp_output_198_0 = tex2DNode192.r - 0.5;
				half Mask_Map232 = tex2D(_Mask_Opaque_Tex,(UV3NoiseMask219).zw).r;
			#if _MASK_FOR_NOISE_ON
				half staticSwitch247 = temp_output_198_0 * Mask_Map232;
			#else
				half staticSwitch247 = temp_output_198_0;
			#endif
				half4 tex2DNode5 = tex2D(_Main_Tex, UV2Tex344.zw + staticSwitch247 * UV2Tex344.x);
				half Noise_r_set413 = tex2DNode192.r;
				half temp_output_470_0 = Mask_Map232 * Noise_r_set413 + 1.0;
				half temp_output_462_0 = UV2Tex344.y *(_Stpe_edg_width + 1.0);
				half stepedg484 = saturate(((( temp_output_470_0 -( temp_output_462_0 *( 1.0 +( 1.0 - _Step_Hardness)))) - _Step_Hardness) /( 1.0 - _Step_Hardness)));
				half4 lerpResult486 = lerp(_Step_edgcolor, (_Main_Color * tex2DNode5 * tex2DNode5.a * _Main_strength * i.ase_color * i.ase_color.a) , stepedg484);
			#if _NOISE_FOR_OPAQUE_NOISE
				half staticSwitch399 = tex2DNode5.a;
			#elif _NOISE_FOR_OPAQUE_STEP
				half step_all483 = saturate(((( temp_output_470_0 -(( temp_output_462_0 - _Stpe_edg_width) *( 1.0 +( 1.0 - _Step_Hardness)))) - _Step_Hardness) /( 1.0 - _Step_Hardness)));
				half staticSwitch399 = tex2DNode5.a * step_all483;
			#elif _NOISE_FOR_OPAQUE_OPAQUE
				half staticSwitch399 = tex2DNode5.a * Noise_r_set413 * Mask_Map232;
			#else
				half staticSwitch399 = tex2DNode5.a;
			#endif
				finalColor = lerpResult486 * saturate(staticSwitch399);
				return finalColor;
			}
			ENDCG
		}
	}
}