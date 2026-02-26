
Shader "Custom/Particle/AdditiveWithMask" {
    Properties{
        _MainTex("Particle Texture", 2D) = "white" {}
        _StencilRef("Stencil Ref", Int) = 1
    }
    SubShader{
        Tags { "Queue" = "Transparent" }
        Stencil {
            Ref[_StencilRef]
            Comp Equal
            Pass Keep
            ReadMask 255
            WriteMask 0
        }
        Blend SrcAlpha One
        ZWrite Off
        Pass {
            SetTexture[_MainTex] { combine texture * primary }
        }
    }
}
