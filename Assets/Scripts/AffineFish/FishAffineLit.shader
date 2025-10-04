Shader "Custom/FishAffineLit_BIRP"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.45

        _Amplitude("Amplitude", Float) = 0.2
        _Frequency("Frequency", Float) = 2.0
        _Speed("Speed", Float) = 2.0
        _RotationAmplitude("Rotation Amplitude (deg)", Float) = 15
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        sampler2D _MainTex, _NormalMap;
        fixed4 _Color;
        half _Metallic, _Smoothness;
        float _Amplitude, _Frequency, _Speed, _RotationAmplitude;

        struct Input { float2 uv_MainTex; float2 uv_NormalMap; };

        float4x4 YRot(float a){ float s=sin(a),c=cos(a); return float4x4(c,0,-s,0, 0,1,0,0, s,0,c,0, 0,0,0,1); }
        float4x4 Translate(float3 t){ return float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, t.x,t.y,t.z,1); }

        void vert(inout appdata_full v)
        {
            float t = _Time.y;
            float along = v.vertex.y;                 // change to .x or .z if needed
            float wave = sin(along*_Frequency + t*_Speed);

            float3 trans = float3(wave*_Amplitude,0,0);
            float rotRad = radians(wave*_RotationAmplitude);

            float4x4 R = YRot(rotRad);
            float4x4 T = Translate(trans);

            v.vertex = mul(mul(T,R), v.vertex);

            float3x3 R3 = (float3x3)R;
            v.normal = normalize(mul(R3, v.normal));
            v.tangent.xyz = normalize(mul(R3, v.tangent.xyz));
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = c.a;
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
        }
        ENDCG
    }

    Fallback "Standard"
}
