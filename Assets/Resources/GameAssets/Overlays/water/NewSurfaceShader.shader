Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_FogDensity("Fog density", Range(0,1)) = 0.0
			_FogColor("Fog color", Color) = (0,0,0,1)
	}
	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		ZWrite Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		uniform sampler2D _CameraDepthTexture;

		sampler2D _BumpMap;

		struct Input {
			float2 uv_BumpMap;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _FogDensity;
		fixed4 _FogColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture,
				UNITY_PROJ_COORD(IN.screenPos)).r);
			float objZ = IN.screenPos.w;
			float dist = sceneZ - objZ;
			float fogFactor = pow(2.718, -dist * _FogDensity);

			fixed4 c = lerp(_FogColor, _Color, fogFactor);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
