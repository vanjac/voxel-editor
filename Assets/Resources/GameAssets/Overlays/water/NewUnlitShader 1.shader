Shader "Unlit/NewUnlitShader 1"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 view_dir : TEXCOORD0;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.view_dir = normalize(WorldSpaceViewDir(v.vertex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (dot(i.normal, i.view_dir) > 0)
					// front face
					return fixed4(1, 0, 0, 1);
				else
					// back face
					return fixed4(0, 1, 0, 1);
			}
			ENDCG
		}
	}
}
