// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:32640,y:32826,varname:node_3138,prsc:2|emission-512-OUT,alpha-6325-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32015,y:32776,ptovrint:False,ptlb:MainTex_Color,ptin:_MainTex_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.6323529,c2:0.6323529,c3:0.6323529,c4:1;n:type:ShaderForge.SFN_TexCoord,id:7001,x:31342,y:33007,varname:node_7001,prsc:2,uv:1,uaff:True;n:type:ShaderForge.SFN_Tex2d,id:8477,x:32015,y:32950,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_8477,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2503-OUT;n:type:ShaderForge.SFN_Append,id:2817,x:31634,y:32922,varname:node_2817,prsc:2|A-7001-U,B-7001-V;n:type:ShaderForge.SFN_Multiply,id:512,x:32288,y:32908,varname:node_512,prsc:2|A-7241-RGB,B-8477-RGB,C-350-RGB,D-4126-RGB;n:type:ShaderForge.SFN_VertexColor,id:350,x:32015,y:33136,varname:node_350,prsc:2;n:type:ShaderForge.SFN_Tex2d,id:4126,x:31998,y:33313,ptovrint:False,ptlb:mask,ptin:_mask,varname:node_4126,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-2541-OUT;n:type:ShaderForge.SFN_Multiply,id:6325,x:32288,y:33150,varname:node_6325,prsc:2|A-7241-A,B-8477-A,C-350-A,D-4126-R;n:type:ShaderForge.SFN_TexCoord,id:9738,x:31348,y:33275,varname:node_9738,prsc:2,uv:2,uaff:True;n:type:ShaderForge.SFN_Append,id:5201,x:31559,y:33306,varname:node_5201,prsc:2|A-9738-Z,B-9738-W;n:type:ShaderForge.SFN_TexCoord,id:3906,x:31342,y:32796,varname:node_3906,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:2503,x:31843,y:32869,varname:node_2503,prsc:2|A-3906-UVOUT,B-2817-OUT;n:type:ShaderForge.SFN_Add,id:2541,x:31778,y:33269,varname:node_2541,prsc:2|A-3906-UVOUT,B-5201-OUT;proporder:7241-8477-4126;pass:END;sub:END;*/

Shader "XuanFu/Particles/Add_Mask_Custom_Flash" {
    Properties {
        _MainTex_Color ("MainTex_Color", Color) = (0.6323529,0.6323529,0.6323529,1)
        _MainTex ("MainTex", 2D) = "white" {}
        _mask ("mask", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _MainTex_Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _mask; uniform float4 _mask_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float2 node_2503 = (i.uv0+float2(i.uv1.r,i.uv1.g));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_2503, _MainTex));
                float2 node_2541 = (i.uv0+float2(i.uv2.b,i.uv2.a));
                float4 _mask_var = tex2D(_mask,TRANSFORM_TEX(node_2541, _mask));
                float3 emissive = (_MainTex_Color.rgb*_MainTex_var.rgb*i.vertexColor.rgb*_mask_var.rgb);
                float3 finalColor = emissive;
                return fixed4(finalColor,(_MainTex_Color.a*_MainTex_var.a*i.vertexColor.a*_mask_var.r));
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
