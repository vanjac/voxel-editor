Shader "Unlit/Invisible"
{
    Properties
    {
		[HideInInspector] _Color ("Color", Color) = (1,1,1,1) // ignored
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }

        Colormask 0
		ZWrite Off

        Pass
        { }
    }
}
