Shader "Custom/CustomSkybox"
{
    Properties
    {
        _FrontTex ("Front (+Z)", 2D) = "white" {}
        _BackTex ("Back (-Z)", 2D) = "white" {}
        _LeftTex ("Left (-X)", 2D) = "white" {}
        _RightTex ("Right (+X)", 2D) = "white" {}
        _UpTex ("Up (+Y)", 2D) = "white" {}
        _DownTex ("Down (-Y)", 2D) = "white" {}

        _VerticalRotation ("Vertical Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FrontTex, _BackTex, _LeftTex, _RightTex, _UpTex, _DownTex;
            float _VerticalRotation;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // 수직 회전 변환
                float angle = _VerticalRotation * UNITY_PI / 180.0;
                float3x3 rotationMatrix = float3x3(
                    cos(angle), 0, sin(angle),
                    0, 1, 0,
                    -sin(angle), 0, cos(angle)
                );

                o.texcoord = mul(rotationMatrix, v.texcoord);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color;

                if (abs(i.texcoord.y) > abs(i.texcoord.x) && abs(i.texcoord.y) > abs(i.texcoord.z))
                {
                    color = i.texcoord.y > 0 ? tex2D(_UpTex, i.texcoord.xz * 0.5 + 0.5) : tex2D(_DownTex, i.texcoord.xz * 0.5 + 0.5);
                }
                else if (abs(i.texcoord.x) > abs(i.texcoord.z))
                {
                    color = i.texcoord.x > 0 ? tex2D(_RightTex, i.texcoord.zy * 0.5 + 0.5) : tex2D(_LeftTex, i.texcoord.zy * 0.5 + 0.5);
                }
                else
                {
                    color = i.texcoord.z > 0 ? tex2D(_FrontTex, i.texcoord.xy * 0.5 + 0.5) : tex2D(_BackTex, i.texcoord.xy * 0.5 + 0.5);
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}