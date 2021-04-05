Shader "Skybox/Skybox Solid Stencil"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }

        Pass {
            Cull Off ZWrite Off
            Stencil {
                Ref 2
                Comp always
                Pass replace
            }
        }
        Pass {
            Cull Off ZWrite Off ZTest Off
            Stencil {
                Ref 2
                Comp equal
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
        
            fixed4 _Color;

            struct appdata_t {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
            };
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}
