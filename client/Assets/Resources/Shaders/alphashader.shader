﻿Shader "ARPG/alpha" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_CutOff("Alpha CutOff",Range(0,1)) =1
	}
	SubShader {
		Tags { 
		    "RenderType"="Transparent" 
		    "IgnoreProjector"="True"  
            "Queue"="Transparent"
            "ForceNoShadowCasting"="True"
             }

		LOD 200
		//CutOff[_CutOff]
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend SrcColor OneMinusSrcColor
		//ZWrite off
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff  

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = .5;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
