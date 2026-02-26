// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:False,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:32792,y:32655,varname:node_4013,prsc:2|alpha-9602-OUT,refract-7629-OUT;n:type:ShaderForge.SFN_Tex2d,id:8366,x:32155,y:32756,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_8366,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-6376-OUT;n:type:ShaderForge.SFN_Append,id:3470,x:32376,y:32798,varname:node_3470,prsc:2|A-8366-R,B-8366-G;n:type:ShaderForge.SFN_Multiply,id:7629,x:32566,y:32958,varname:node_7629,prsc:2|A-3470-OUT,B-4672-A,C-5674-OUT,D-8366-A;n:type:ShaderForge.SFN_Vector1,id:9602,x:32389,y:32666,varname:node_9602,prsc:2,v1:0;n:type:ShaderForge.SFN_VertexColor,id:4672,x:32155,y:32938,varname:node_4672,prsc:2;n:type:ShaderForge.SFN_Slider,id:5674,x:32237,y:33166,ptovrint:False,ptlb:node_5674,ptin:_node_5674,varname:node_5674,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:1;n:type:ShaderForge.SFN_TexCoord,id:7665,x:31409,y:32688,varname:node_7665,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_ComponentMask,id:4331,x:31655,y:32559,varname:node_4331,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-7665-U;n:type:ShaderForge.SFN_ComponentMask,id:1598,x:31737,y:32878,varname:node_1598,prsc:2,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-7665-V;n:type:ShaderForge.SFN_Multiply,id:9887,x:31713,y:32744,varname:node_9887,prsc:2|A-489-TSL,B-6629-OUT;n:type:ShaderForge.SFN_Time,id:489,x:31343,y:32866,varname:node_489,prsc:2;n:type:ShaderForge.SFN_ValueProperty,id:6629,x:31474,y:32996,ptovrint:False,ptlb:U,ptin:_U,varname:node_6629,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Add,id:5175,x:31881,y:32603,varname:node_5175,prsc:2|A-4331-OUT,B-9887-OUT;n:type:ShaderForge.SFN_Add,id:2046,x:31908,y:32888,varname:node_2046,prsc:2|A-1598-OUT,B-3841-OUT;n:type:ShaderForge.SFN_Multiply,id:3841,x:31710,y:33054,varname:node_3841,prsc:2|A-489-TSL,B-9209-OUT;n:type:ShaderForge.SFN_ValueProperty,id:9209,x:31464,y:33121,ptovrint:False,ptlb:V,ptin:_V,varname:node_9209,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:6376,x:31985,y:32713,varname:node_6376,prsc:2|A-5175-OUT,B-2046-OUT;proporder:8366-6629-9209-5674;pass:END;sub:END;*/

Shader "CBB/CBB_niuqu" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _U ("U", Float ) = 0
        _V ("V", Float ) = 0
        _node_5674 ("node_5674", Range(0, 1)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _node_5674;
            uniform float _U;
            uniform float _V;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float4 node_489 = _Time;
                float2 node_6376 = float2((i.uv0.r.r+(node_489.r*_U)),(i.uv0.g.r+(node_489.r*_V)));
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_6376, _Texture));
                float2 sceneUVs = (i.projPos.xy / i.projPos.w) + (float2(_Texture_var.r,_Texture_var.g)*i.vertexColor.a*_node_5674*_Texture_var.a);
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
////// Lighting:
                float3 finalColor = 0;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,0.0),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
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
            #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
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
//    CustomEditor "ShaderForgeMaterialInspector"
}
