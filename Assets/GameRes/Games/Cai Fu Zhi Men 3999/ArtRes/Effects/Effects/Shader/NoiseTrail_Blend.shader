Shader "S_Game_Effects/NoiseTrail_Blend" {
    Properties {
		[Toggle]_VertexColorMix("Vertex Color Mix", float) = 0
        [HDR] _MainTexColor ("MainTex Color", Color) = (0.5,0.5,0.5,1)
		[WrapMode]_MainTex_Wrap("MainTex Wrap Mode", Float) = 0
        _MainTex ("MainTex", 2D) = "white" {}
        _MainTex_ScrollX ("MainTex_ScrollX", Float ) = -0.5
        _MainTex_ScrollY ("MainTex_ScrollY", Float ) = 0.5
		[WrapMode]_Mask_Wrap("Mask Wrap Mode", Float) = 0
        _Mask ("Mask", 2D) = "white" {}
        _Mask_ScrollX ("Mask_ScrollX", Float ) = -0.5
        _Mask_ScrollY ("Mask_ScrollY", Float ) = 0.5
        _NoisePower ("Noise Power", Float ) = 0.2
		[Header(Render State)]
		[Enum(Off, 0, Front, 1, Back, 2)]_CullMode("Cull Mode", float) = 2
		_PolygonOffset("Polygon Offset", float) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha, Zero One
            ZWrite Off
			Cull [_CullMode]
			Offset [_PolygonOffset], [_PolygonOffset]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _MainTex_Wrap;
			uniform fixed _VertexColorMix;
            uniform fixed4 _MainTexColor;
			uniform sampler2D _Mask;
			uniform float4 _Mask_ST;
			uniform float _Mask_Wrap;
            uniform float _NoisePower;
			uniform float _MainTex_ScrollX;
			uniform float _MainTex_ScrollY;
            uniform float _Mask_ScrollX;
            uniform float _Mask_ScrollY;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
				fixed4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
				fixed4 vertexColor : COLOR;
            };

			float2 WrapMode(float2 uv, float mode) // mode 0=Repeat 1=Clamp
			{
				return lerp(uv, saturate(uv), mode);
			}

			fixed4 LerpOneTo4(fixed4 b, fixed4 t)
			{
				// lerp(1, b, t)
				fixed4 oneMinusT = fixed4(1, 1, 1, 1) - t;
				return oneMinusT + b * t;
			}

            VertexOutput vert (VertexInput v)
			{
                VertexOutput o = (VertexOutput)0;
				o.uv = v.texcoord0.xyxy;
				o.uv += float4(_MainTex_ScrollX, _MainTex_ScrollY, _Mask_ScrollX, _Mask_ScrollY) * _Time.y;
				o.uv.zw = TRANSFORM_TEX((o.uv.zw), _Mask);
				o.vertexColor = _MainTexColor;
				o.vertexColor *= LerpOneTo4(v.vertexColor, fixed4(_VertexColorMix.xxx, 1));
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR
			{
				float2 mainUV = i.uv.xy;
				float2 maskUV = WrapMode(i.uv.zw, _Mask_Wrap);
                float2 mask = tex2D(_Mask, maskUV).xy;
				mainUV += float2(mask.x, 1 - mask.y) * _NoisePower;
				mainUV = TRANSFORM_TEX(mainUV, _MainTex);
				mainUV = WrapMode(mainUV, _MainTex_Wrap);

				fixed4 color = tex2D(_MainTex, mainUV);
				color *= i.vertexColor;
				return color;
            }
            ENDCG
        }
    }
    FallBack Off
}
