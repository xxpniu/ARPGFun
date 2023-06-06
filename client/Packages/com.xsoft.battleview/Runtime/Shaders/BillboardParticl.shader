Shader "Billboard/BillboardParticl" 
    {
        Properties 
        {
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _Disappear( "Disappear", Range(0, 20) ) = 8
            _Life("Life", float) = 1.5
            _Speed("Speed", float) = 2
            _Acce("Acce", float) = -0.9
            _B("Scale time", float) = 0.125
            _C("Scale size", float) = 1
        }

        Subshader 
        {
            Tags 
			{ 
				"RenderPipeline" = "UniversalPipeline"
				"Queue"="Overlay" 
				"IgnoreProjector"="True"
				"RenderType"="Transparent" 
				"ShaderModel"="2.0"
		    }

            Pass 
            {
				Name "ForwardLit"
                Tags { "LightMode" = "UniversalForward" }
				Cull Off
				Blend SrcAlpha  OneMinusSrcAlpha
				ZTest Always
				ZWrite Off

				HLSLPROGRAM

                
				#pragma vertex vert
				#pragma fragment frag
				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 3.0
				#pragma multi_compile_instancing
				#pragma fragmentoption ARB_precision_hint_fastest
                #pragma glsl_no_auto_normalization

				#include "billpass.hlsl"
                ENDHLSL
            }
        }
    }