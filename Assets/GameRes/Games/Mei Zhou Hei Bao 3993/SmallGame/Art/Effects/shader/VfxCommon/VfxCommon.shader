/*--------------------------------------
* @description: VFXCommon
* @author XC
* @date 2026/06/17
* @update: 重新映射CustomData: 
* CustomData1.xy(i.uv.zw) -> 溶解贴图UV偏移
* CustomData1.z(i.uv1.x) -> 溶解进度
* CustomData2.xy(i.uv1.zw) -> 主贴图UV偏移
* CustomData2.zw(i.uv2.xy) -> 遮罩UV偏移
*--------------------------------------*/
Shader "XuanFu/Particles/VfxCommon"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_MainColor("MainColor",color) = (1,1,1,1)
		_MainTexSpeed_x("MainTexSpeed_x", Float) = 0
		_MainTexSpeed_y("MainTexSpeed_y", Float) = 0
		
		[Toggle(_USE_DISTURBANCE)]_UseDist("UseDisturbance",int)=0
		_DisturbanceTex("DisturbanceTex", 2D) = "black" {}
		_DistSpeed_x("DistSpeed_x", Float) = 0
		_DistSpeed_y("DistSpeed_y", Float) = 0
		
		[Toggle(_USE_SECOND_DISTURBANCE)]_UseSecondDist("UseSecondDisturbance",int)=0
		_DisturbanceTex01("DisturbanceTex01", 2D) = "black" {}
		_DistSpeed01_x("DistSpeed_x", Float) = 0
		_DistSpeed01_y("DistSpeed_y", Float) = 0
		
		[Toggle(_USE_DISTURBANCE_MASK)]_UseDistMask("UseDisturbanceMask",int)=0
		_DistMask("DistMask", 2D) = "white" {}
		_Disturbance_Pow("Disturbance_Pow", Float) = 0
		
		[Toggle(_USE_MASK)]_UseMask("UseMask",int)=0
		_MaskTex("MaskTex", 2D) = "white" {}
		_MaskSpeed_x("MaskSpeed_x",Float) = 0.0
        _MaskSpeed_y("MaskSpeed_y",Float) = 0.0
		_Mask_Percentage("MaskPercentage", Range(-1 , 1)) = 0
		_MaskSoft("MaskSoft",Float) = 0
		
		[Toggle(_USE_DISSOlVE)]_UseDissolve("UseDissolve",int)=0
		_DissolveTex("DissolveTex", 2D) = "white" {}
		_DissolveSpeed_x("DissolveSpeed_x",float)=0
		_DissolveSpeed_y("DissolveSpeed_y",float)=0
		
		[Toggle(_USE_SECOND_DISSOlVE)]_UseSecondDissolve("UseSecondDissolve",int)=0
		_DissolveTex01("DissolveTex01", 2D) = "black" {}
		_DissolveSpeed01_x("DissolveSpeed01_x",float)=0
		_DissolveSpeed01_y("DissolveSpeed01_y",float)=0
		
		[Toggle(_USE_DISSOlVE_MASK)]_UseDissolveMask("UseDissolveMask",int)=0
		_DissolveMask("DissolveMask", 2D) = "white" {}
		_Dissolve_Soft("Dissolve_Soft", Float) = 0.0
		_DissEdgeRange("DissEdgeRange",Float) = 0.0
		_DissEdgeRangeSoft("DissEdgeRangeSoft",Float) = 0.0
		[HDR]_DissEdgeColor("DissEdgeColor",Color) = (0.5,0.5,0.5,1)

        // 新增：用于控制是否读取 CustomData2 数据流的宏开关
		[Toggle(_USE_CUSTOM2)]_UseCustom2("Use CustomData2 (Main/Mask Offset)", int) = 0

		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 0
		[Toggle]_Zwrite("Zwrite", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent""Queue"="Transparent" }
		LOD 100
		Cull [_CullMode]
		ZWrite [_Zwrite]
		Lighting Off
		ZTest LEqual
		Blend [_Src] [_Dst]
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma shader_feature _USE_DISTURBANCE
			#pragma shader_feature _USE_SECOND_DISTURBANCE
			#pragma shader_feature _USE_DISTURBANCE_MASK
			#pragma shader_feature _USE_MASK
			#pragma shader_feature _USE_DISSOlVE
			#pragma shader_feature _USE_SECOND_DISSOlVE
			#pragma shader_feature _USE_DISSOlVE_MASK
			#pragma shader_feature _USE_CUSTOM2 // 注册 CustomData2 的变体宏
			#pragma multi_compile_instancing
			#define ADDALPHA(col)  _Src*_Dst==1||_Src*_Dst==4?col.xyz*col.w:col.xyz
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex:POSITION;
				float4 uv:TEXCOORD0;
				float4 uv1:TEXCOORD1;
				float4 uv2:TEXCOORD2; 
				float4 color:COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 uv1:TEXCOORD1;
				float4 uv2:TEXCOORD2;
				float4 vertex : SV_POSITION;
				float4 color:TEXCOORD3; 
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainColor;
			float _MainTexSpeed_x;
			float _MainTexSpeed_y;
			float _Src;
			float _Dst;

			#ifdef _USE_DISTURBANCE
			sampler2D _DisturbanceTex;
			float4 _DisturbanceTex_ST;
			float _DistSpeed_x;
			float _DistSpeed_y;
			#endif

			#ifdef _USE_SECOND_DISTURBANCE
			sampler2D _DisturbanceTex01;
			float4 _DisturbanceTex01_ST;
			float _DistSpeed01_x;
			float _DistSpeed01_y;
			#endif

			#ifdef _USE_DISTURBANCE_MASK
			sampler2D _DistMask;
			float4 _DistMask_ST;
			#endif
			float _Disturbance_Pow;

			#ifdef _USE_MASK
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			float _MaskSpeed_x;
			float _MaskSpeed_y;
			float _Mask_Percentage;
			float _MaskSoft;
			#endif

			#ifdef _USE_DISSOlVE
			sampler2D _DissolveTex;
			float4 _DissolveTex_ST;
			float _Dissolve_Soft;
			float _DissEdgeRange;
			float _DissEdgeRangeSoft;
			float _DissolveSpeed_x;
			float _DissolveSpeed_y;
			#endif

			#ifdef _USE_SECOND_DISSOlVE
			sampler2D _DissolveTex01;
			float4 _DissolveTex01_ST;
			float _DissolveSpeed01_x;
			float _DissolveSpeed01_y;	
			#endif

			#ifdef _USE_DISSOlVE_MASK
			sampler2D _DissolveMask;
			float4 _DissolveMask_ST;
			#endif
			float4 _DissEdgeColor;

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv1 = v.uv1;
				o.uv2 = v.uv2;
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{ 
				UNITY_SETUP_INSTANCE_ID(i);
				
				// 1. 遮罩逻辑：安全读取 CustomData2.zw (i.uv2.xy)
				#ifdef _USE_MASK
                    #ifdef _USE_CUSTOM2
					    float2 maskScroll = float2(_MaskSpeed_x, _MaskSpeed_y) * _Time.y + i.uv2.xy;
                    #else
                        float2 maskScroll = float2(_MaskSpeed_x, _MaskSpeed_y) * _Time.y;
                    #endif
					float2 uv_mask = TRANSFORM_TEX(i.uv.xy, _MaskTex) + maskScroll;
					float mask = tex2D(_MaskTex, uv_mask).r;
					half maskValue=smoothstep(_Mask_Percentage, (_Mask_Percentage+_MaskSoft), mask);
				#else
				    half maskValue=1;
				#endif

				// 2. 扰动逻辑
				#ifdef _USE_DISTURBANCE
				    float2 distScroll = float2(_DistSpeed_x, _DistSpeed_y) * _Time.y;
				    float2 uv_dist = TRANSFORM_TEX(i.uv.xy, _DisturbanceTex) + distScroll;
				    float dist=tex2D(_DisturbanceTex, uv_dist).r;
				    #ifdef _USE_SECOND_DISTURBANCE
					    float2 distScroll01 = float2(_DistSpeed01_x, _DistSpeed01_y) * _Time.y;
					    float2 uv_dist1 = TRANSFORM_TEX(i.uv.xy,_DisturbanceTex01) + distScroll01;
					    dist=max(dist,tex2D(_DisturbanceTex01, uv_dist1).r);
					#endif
					#ifdef _USE_DISTURBANCE_MASK
					    float2 uv_DistMask=TRANSFORM_TEX(i.uv.xy, _DistMask);
					    float distMask=tex2D(_DistMask, uv_DistMask).r;
					    dist = dist*distMask;
					#endif
				#else
				    float dist=0;
				#endif

				// 3. 溶解逻辑：完全由 CustomData1 控制
				#ifdef _USE_DISSOlVE
				    float DissolveMask=0;
				    // 使用 i.uv.zw (对应 CustomData1.xy) 控制溶解贴图 UV 偏移
				    float2 dissScroll = float2(_DissolveSpeed_x, _DissolveSpeed_y) * _Time.y + i.uv.zw;
				    float2 uv_DissolveTex = TRANSFORM_TEX(i.uv.xy, _DissolveTex) + dissScroll;
				    float Dissolve = tex2D(_DissolveTex, uv_DissolveTex).r;
				    
                    #ifdef _USE_SECOND_DISSOlVE
					    float2 dissScroll01 = float2(_DissolveSpeed01_x, _DissolveSpeed01_y) * _Time.y + i.uv.zw;
					    float2 uv_DissolveTex01 = TRANSFORM_TEX(i.uv.xy, _DissolveTex01) + dissScroll01;
					    float Dissolve01 = tex2D(_DissolveTex01, uv_DissolveTex01).r;
					    Dissolve=max(Dissolve,Dissolve01);
					#endif
					
					#ifdef _USE_DISSOlVE_MASK
					    float2 uv_DissolveMask = TRANSFORM_TEX(i.uv.xy, _DissolveMask);
					    DissolveMask=1-tex2D(_DissolveMask,uv_DissolveMask).r;
					#endif
				
				    // 使用 i.uv1.x (对应 CustomData1.z) 控制溶解进度，不受 CustomData2 状态影响
				    float diss = smoothstep(i.uv1.x + DissolveMask, (i.uv1.x + DissolveMask + _Dissolve_Soft), Dissolve);
				    float dissEdge = smoothstep(_DissEdgeRange + i.uv1.x + DissolveMask, _DissEdgeRange + i.uv1.x + DissolveMask + _DissEdgeRangeSoft, Dissolve);
				#else
				    half diss=1;
				    half dissEdge=1;
				#endif

                // 4. 主贴图逻辑：安全读取 CustomData2.xy (i.uv1.zw)
                #ifdef _USE_CUSTOM2
				    float2 mainScroll = float2(_MainTexSpeed_x, _MainTexSpeed_y) * _Time.y + i.uv1.zw;
                #else
                    float2 mainScroll = float2(_MainTexSpeed_x, _MainTexSpeed_y) * _Time.y;
                #endif
				float2 uv = TRANSFORM_TEX(i.uv.xy, _MainTex) + dist *_Disturbance_Pow + mainScroll - float2(0,1);
				
				float4 col = tex2D(_MainTex,uv);
				half4 final;
				final.rgb= lerp(_DissEdgeColor.rgb,col.rgb*_MainColor.rgb,dissEdge)*i.color.rgb;
				final.a=col.a*maskValue*diss*_MainColor.a*i.color.a;
				final.rgb=ADDALPHA(final);
				return final;
			}
			ENDCG
		}
	}
	CustomEditor "VfxCommonGUI"
}