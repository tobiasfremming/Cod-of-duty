Shader "Unlit/FishAffineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Amplitude ("Amplitude", Float) = 0.2
        _Frequency ("Frequency", Float) = 2.0
        _Speed ("Speed", Float) = 2.0

        _RotationAmplitude ("Rotation Amplitude (deg)", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Amplitude;
            float _Frequency;
            float _Speed;
            float _TimeY;
            float _RotationAmplitude;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // Rotation matrix around Y
            float4x4 YRotation(float angleRadians)
            {
                float s = sin(angleRadians);
                float c = cos(angleRadians);
                return float4x4(
                    c, 0, -s, 0,
                    0, 1,  0, 0,
                    s, 0,  c, 0,
                    0, 0,  0, 1
                );
            }

            // Translation matrix
            float4x4 Translate(float3 t)
            {
                return float4x4(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    t.x, t.y, t.z, 1
                );
            }



            v2f vert (appdata v)
            {
                v2f o;
                // Compute time-based sine wave
                float t = _Time.y;
                float wave = sin(v.vertex.y * _Frequency + t * _Speed);
                // Translation along X axis (simulate swimming side to side)
                float3 translation = float3(wave * _Amplitude, 0, 0);

                // Rotation around Y axis
                float rotDegrees = wave * _RotationAmplitude;
                float rotRadians = radians(rotDegrees);

                float4x4 T = Translate(translation);
                float4x4 R = YRotation(rotRadians);

                // Combine: first rotate, then translate
                float4x4 TR = mul(T, R);

                float4 newPos = mul(TR, v.vertex);

                o.vertex = UnityObjectToClipPos(newPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;


            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
