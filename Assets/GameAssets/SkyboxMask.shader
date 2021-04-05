Shader "FX/SkyboxMask"
{
	SubShader
	{
		// render immediately before skybox
		Tags{ "Queue"="AlphaTest+50" }

		ColorMask 0
		ZWrite On

		Pass
		{
			Stencil
			{
				Ref 2
				Comp always
				Pass replace
			}
		}
	}
}
