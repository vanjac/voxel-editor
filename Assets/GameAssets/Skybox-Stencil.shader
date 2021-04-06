// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/6 Sided Stencil" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _FrontTex ("Front [+Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _BackTex ("Back [-Z]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _LeftTex ("Left [+X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _RightTex ("Right [-X]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _UpTex ("Up [+Y]   (HDR)", 2D) = "grey" {}
    [NoScaleOffset] _DownTex ("Down [-Y]   (HDR)", 2D) = "grey" {}
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

        half4 _Tint;
        half _Exposure;
        float _Rotation;

        sampler2D _FrontTex;
        sampler2D _BackTex;
        sampler2D _LeftTex;
        sampler2D _RightTex;
        sampler2D _UpTex;
        sampler2D _DownTex;

        half4 _FrontTex_HDR;
        half4 _BackTex_HDR;
        half4 _LeftTex_HDR;
        half4 _RightTex_HDR;
        half4 _UpTex_HDR;
        half4 _DownTex_HDR;

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
        };
        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
        };
        v2f vert (appdata_t v)
        {
            v2f o;
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }
        half4 skybox_frag (float2 uv, sampler2D smp, half4 smpDecode)
        {
            half4 tex = tex2D (smp, uv / 2 + 0.5);
            half3 c = DecodeHDR (tex, smpDecode);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;
            return half4(c, 1);
        }

        half4 skybox_x (float3 coord) {
            float2 uv = coord.zy / coord.x;
            if (coord.x > 0)
                return skybox_frag(float2(-1, 1) * uv, _LeftTex, _LeftTex_HDR);
            else
                return skybox_frag(-uv, _RightTex, _RightTex_HDR);
        }

        half4 skybox_y (float3 coord) {
            float2 uv = coord.xz / coord.y;
            if (coord.y > 0)
                return skybox_frag(float2(1, -1) * uv, _UpTex, _UpTex_HDR);
            else
                return skybox_frag(-uv, _DownTex, _DownTex_HDR);
        }

        half4 skybox_z (float3 coord) {
            float2 uv = coord.xy / coord.z;
            if (coord.z > 0)
                return skybox_frag(uv, _FrontTex, _FrontTex_HDR);
            else
                return skybox_frag(float2(1, -1) * uv, _BackTex, _BackTex_HDR);
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
