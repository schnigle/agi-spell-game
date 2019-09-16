Shader "Custom/Translucent" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_SubColor ("Subsurface color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Intensity ("Subsurface intensity", Range(0, 10)) = 1
		_Cutoff ("Alpha cut off", Range(0, 1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="TransparentCutout" }
		Tags { "Queue"="Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Translucent alphatest:_Cutoff addshadow
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color, _SubColor;
		float _Intensity;

		struct Input {
			float2 uv_MainTex;
		};

		#include "UnityPBSLighting.cginc"
		inline fixed4 LightingTranslucent(SurfaceOutputStandard s, fixed3 viewDir, UnityGI gi)
		{
			// Get standard lighting color
			fixed4 pbr = LightingStandard(s, viewDir, gi);
			float atten = gi.light.color;	// Get attenuation from GI
			half NdotL = max(0, dot(s.Normal, -gi.light.dir));
			// Add subsurface scattering color to standard light color.
			half4 transAlbedo;
			transAlbedo.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten) * _Intensity * _SubColor.rbg;
			transAlbedo.a = 1;
			transAlbedo.rgb = pbr.rgb + transAlbedo.rgb;
			return transAlbedo;
		}

		void LightingTranslucent_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Alpha = tex.a;
		}

		ENDCG
	}
	FallBack "Diffuse"
}