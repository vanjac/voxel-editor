// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/6 Sided Stencil" {
Properties {
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _FrontTex ("Front (+Z)", 2D) = "white" {}
    [NoScaleOffset] _BackTex ("Back (-Z)", 2D) = "white" {}
    [NoScaleOffset] _LeftTex ("Left (+X)", 2D) = "white" {}
    [NoScaleOffset] _RightTex ("Right (-X)", 2D) = "white" {}
    [NoScaleOffset] _UpTex ("Up (+Y)", 2D) = "white" {}
    [NoScaleOffset] _DownTex ("Down (-Y)", 2D) = "white" {}
}

SubShader {
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
        #pragma target 2.0
        #include "UnityCG.cginc"
        
        sampler2D _FrontTex;
        sampler2D _BackTex;
        sampler2D _LeftTex;
        sampler2D _RightTex;
        sampler2D _UpTex;
        sampler2D _DownTex;
        
        float _Rotation;

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }
        half4 skybox_frag (float2 uv, sampler2D smp)
        {
            return tex2D (smp, uv / 2 + 0.5);
        }

        half4 skybox_x (float3 coord) {
            float2 uv = coord.zy / coord.x;
            if (coord.x > 0)
                return skybox_frag(float2(-1, 1) * uv, _LeftTex);
            else
                return skybox_frag(-uv, _RightTex);
        }

        half4 skybox_y (float3 coord) {
            float2 uv = coord.xz / coord.y;
            if (coord.y > 0)
                return skybox_frag(float2(1, -1) * uv, _UpTex);
            else
                return skybox_frag(-uv, _DownTex);
        }

        half4 skybox_z (float3 coord) {
            float2 uv = coord.xy / coord.z;
            if (coord.z > 0)
                return skybox_frag(uv, _FrontTex);
            else
                return skybox_frag(float2(1, -1) * uv, _BackTex);
        }

        half4 frag (v2f i) : SV_Target {
            float3 absCoord = abs(i.texcoord);
            if (absCoord.x > absCoord.y) {
                if (absCoord.x > absCoord.z) {
                    return skybox_x(i.texcoord);
                } else {
                    return skybox_z(i.texcoord);
                }
            } else {
                if (absCoord.y > absCoord.z) {
                    return skybox_y(i.texcoord);
                } else {
                    return skybox_z(i.texcoord);
                }
            }
        }
        ENDCG
    }
}
}
