Shader "Custom/FishAffineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _NormalMap  ("Normal Map", 2D) = "bump" {}
        _Metallic   ("Metallic", Range(0,1)) = 1.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.45
        _Color      ("Color", Color) = (1,1,1,1)


        _Amplitude ("Amplitude", Float) = 0.2
        _Frequency ("Frequency", Float) = 2.0
        _Speed ("Speed", Float) = 2.0
        _VelocityScale ("Velocity Scale", Float) = 1.0
        
        [Header(Rotation)]
        _RotationX ("Rotation X (deg)", Float) = 0
        _RotationY ("Rotation Y (deg)", Float) = 15
        _RotationZ ("Rotation Z (deg)", Float) = 0
        
        [Header(Shear)]
        _ShearXY ("Shear XY", Float) = 0.3
        _ShearXZ ("Shear XZ", Float) = 0
        _ShearYX ("Shear YX", Float) = 0
        _ShearYZ ("Shear YZ", Float) = 0
        _ShearZX ("Shear ZX", Float) = 0
        _ShearZY ("Shear ZY", Float) = 0
        
        [Header(Shear Falloff)]
        _ShearFalloffAxis ("Falloff Axis (0=X 1=Y 2=Z)", Int) = 0
        _ShearFalloffStrength ("Falloff Strength", Range(0,5)) = 1.0
        
        [Header(Fresnel Shine)]
        _RimColor ("Rim Color", Color) = (0.9, 0.9, 1.0, 1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 4.0
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.5
        
        [Header(Specular)]
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Shininess ("Shininess", Range(1, 128)) = 32
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 0.5
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
            #pragma multi_compile_fwdbase       // NEW: get _LightColor0 variants

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Amplitude;
            float _Frequency;
            float _Speed;
            float _VelocityScale;
            
            float _RotationX;
            float _RotationY;
            float _RotationZ;
            
            float _ShearXY;
            float _ShearXZ;
            float _ShearYX;
            float _ShearYZ;
            float _ShearZX;
            float _ShearZY;
            
            int _ShearFalloffAxis;
            float _ShearFalloffStrength;
            
            fixed4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            
            fixed4 _SpecularColor;
            float _Shininess;
            float _SpecularIntensity;


            sampler2D _NormalMap;
            fixed4 _Color;
            half _Metallic;
            half _Smoothness;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_NormalMap;
            };


            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 posWS    : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                float4 vertex   : SV_POSITION;
            };

            // Rotation matrix around X
            float4x4 XRotation(float angleRadians)
            {
                float s = sin(angleRadians);
                float c = cos(angleRadians);
                return float4x4(
                    1,  0, 0, 0,
                    0,  c, s, 0,
                    0, -s, c, 0,
                    0,  0, 0, 1
                );
            }

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

            // Rotation matrix around Z
            float4x4 ZRotation(float angleRadians)
            {
                float s = sin(angleRadians);
                float c = cos(angleRadians);
                return float4x4(
                    c,  s, 0, 0,
                   -s,  c, 0, 0,
                    0,  0, 1, 0,
                    0,  0, 0, 1
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

            // Full shear matrix
            float4x4 ShearMatrix(float xy, float xz, float yx, float yz, float zx, float zy)
            {
                return float4x4(
                    1,  yx, zx, 0,
                    xy, 1,  zy, 0,
                    xz, yz, 1,  0,
                    0,  0,  0,  1
                );
            }



            v2f vert (appdata v)
            {
                v2f o;
                // Compute time-based sine wave, scaled by velocity
                float t = _Time.y;
                float wave = sin(v.vertex.x * _Frequency + t * _Speed * _VelocityScale);
                
                // Translation along X axis (simulate swimming side to side)
                float3 translation = float3(wave * _Amplitude, 0, 0);

                // Rotations (all three axes, controlled by wave)
                float rotX = radians(wave * _RotationX);
                float rotY = radians(wave * _RotationY);
                float rotZ = radians(wave * _RotationZ);

                // Calculate shear falloff based on position along chosen axis
                float posAlongAxis = 0;
                if (_ShearFalloffAxis == 0) posAlongAxis = v.vertex.x;
                else if (_ShearFalloffAxis == 1) posAlongAxis = v.vertex.y;
                else posAlongAxis = v.vertex.z;
                
                // Falloff multiplier for shear (more positive = more shear)
                float shearFalloff = posAlongAxis * _ShearFalloffStrength;

                // Shear values (all six shear components with falloff)
                float shearXY = wave * _ShearXY * shearFalloff;
                float shearXZ = wave * _ShearXZ * shearFalloff;
                float shearYX = wave * _ShearYX * shearFalloff;
                float shearYZ = wave * _ShearYZ * shearFalloff;
                float shearZX = wave * _ShearZX * shearFalloff;
                float shearZY = wave * _ShearZY * shearFalloff;

                // Build transformation matrices
                float4x4 T = Translate(translation);
                float4x4 RX = XRotation(rotX);
                float4x4 RY = YRotation(rotY);
                float4x4 RZ = ZRotation(rotZ);
                float4x4 S = ShearMatrix(shearXY, shearXZ, shearYX, shearYZ, shearZX, shearZY);

                // Combine: Shear -> Rotate (Z, Y, X order) -> Translate
                float4x4 Transform = mul(T, mul(RX, mul(RY, mul(RZ, S))));

                float4 newPos = mul(Transform, v.vertex);

                o.vertex = UnityObjectToClipPos(newPos);

                // Transform normal - need to apply same transformations as vertex
                // For normals, we use the inverse-transpose of the transformation matrix
                // For rotations and uniform scales, this simplifies to just the rotation part
                float3x3 normalTransform = (float3x3)mul(RX, mul(RY, RZ));
                float3 nOS = mul(normalTransform, v.normal);

                // Convert to world space and negate (based on your original shader)
                float3 posWS = mul(unity_ObjectToWorld, newPos).xyz;
                float3 nWS   = UnityObjectToWorldNormal(nOS);

                o.posWS    = posWS;
                o.normalWS = -nWS; // Negated for correct lighting direction

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                // return o;


            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize interpolated normal in fragment shader for better quality
                float3 normalWS = normalize(i.normalWS);
                
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;

                // Calculate view direction (needed for specular and rim)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.posWS);
                
                // Diffuse (Lambert) lighting
                float3 L = normalize(_WorldSpaceLightPos0.xyz);
                float  NdotL = saturate(dot(normalWS, -L));
                float3 diffuse = albedo.rgb * _LightColor0.rgb * NdotL;
                
                // Specular (Blinn-Phong) for smooth metallic highlights
                float3 halfDir = normalize(-L + viewDir);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, _Shininess) * _SpecularIntensity;
                float3 specularColor = _SpecularColor.rgb * specular * _LightColor0.rgb;

                // Ambient lighting from environment
                float3 ambient = ShadeSH9(float4(normalWS, 1.0)).rgb * albedo.rgb;

                // Rim lighting (Fresnel) for silver shine on edges
                float rim = 1.0 - saturate(dot(viewDir, normalWS));
                
                // Make rim more selective - only show on actual edges
                rim = pow(rim, _RimPower);
                
                // Sharpen the rim effect with smoothstep for more control
                rim = smoothstep(0.5, 1.0, rim);
                
                float3 rimColor = _RimColor.rgb * rim * _RimIntensity;

                // Combine all lighting
                float3 finalRGB = diffuse + specularColor + ambient + rimColor;

                fixed4 col = fixed4(finalRGB, albedo.a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
