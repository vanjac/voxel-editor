Shader "FX/SkyboxMask"
{
	SubShader{
		// Render before everything

		Tags{ "Queue" = "Background" }

		// Don't draw in the RGBA channels; just the depth buffer

		ColorMask 0
		ZWrite On

		// Do nothing specific in the pass:

		Pass{}
	}
}
