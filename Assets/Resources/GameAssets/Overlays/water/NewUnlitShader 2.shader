Shader "Unlit/NewUnlitShader 2"
{
	Properties
	{
	}

	CGINCLUDE

#include "UnityCG.cginc"


	struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float3 normal : NORMAL;
		float4 screenPos : TEXCOORD0;
	};

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.normal = v.normal;
		o.screenPos = ComputeScreenPos(o.vertex);
		return o;
	}

	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100


		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite On

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

			fixed4 frag(v2f i) : SV_Target
			{
				// back face
				return fixed4(i.normal.x / 2 + .5,
				i.normal.y / 2 + .5,
				i.normal.z / 2 + .5, 1);
			}
			ENDCG
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

			uniform sampler2D _CameraDepthTexture;
			
			fixed4 frag (v2f i) : SV_Target
			{
				// front face
				float sceneZ = LinearEyeDepth(tex2Dproj(_CameraDepthTexture,
					UNITY_PROJ_COORD(i.screenPos)).r);
				float objZ = i.screenPos.w;

				fixed4 col = fixed4(1.0, 1.0, 1.0, (sceneZ - objZ) / 5);
				return col;
			}
			ENDCG
		}




		
	}
	FallBack "Diffuse"
}
