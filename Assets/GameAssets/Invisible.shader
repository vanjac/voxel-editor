Shader "Unlit/Invisible"
{
    Properties
    { }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }

        Colormask 0
		ZWrite Off

        Pass
        { }
    }
}
