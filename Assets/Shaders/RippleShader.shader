Shader "Custom/RippleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Ripple Center", Vector) = (0.0, 0.0, 0)
        _Target ("Ripple Target", Vector) = (0.0, 0.0, 0)
        _Angle ("Ripple Angle", float) = 1
        _FallOff("Falloff Modifier", float) = 0.5
        _Speed ("Ripple Speed", Float) = 0.0
        _GrayScale ("Gray Scale", Vector) = (1.0 ,1.0 ,1.0)
        _Brightness ("Brightness", Float) = 0.0
        _Intensity ("Intensity", Float) = 0.5
        _Frequency ("Frequency", Float) = 0.01
        _FoveaSize ("FoveaSize", Float) = 0.01
        _OneEye ("OneEye", int) = 0
        _StereoInverse ("Stereo Inverse", int) = 0
        _Direction("Direction", int) = 4
        _DirectionRange("DirectionRange", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _GrayScale;
            float2 _Center;
            float2 _Target;
            float _Speed;
            float _Brightness;
            float _Intensity;
            float _Frequency;
            float _Angle;
            float _FallOff;
            float _FoveaSize;
            // if we only want to display the ripple in one eye
            int _OneEye;
            // if we want to delay the amplitude by pi in one eye
            int _StereoInverse;


            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Helper function
            float f(float t) {
                return t > 0.008856 ? pow(t, 1.0 / 3.0) : (7.787 * t) + (16.0 / 116.0);
            }

            float3 RGBtoCIELAB(float3 color)
            {
                // sRGB to Linear RGB
                float3 linearRGB = pow(color, 2.2);

                // Linear RGB to XYZ (sRGB D65)
                const float3x3 RGBtoXYZ = float3x3(
                    0.4124, 0.3576, 0.1805,
                    0.2126, 0.7152, 0.0722,
                    0.0193, 0.1192, 0.9505
                );
                float3 xyz = mul(RGBtoXYZ, linearRGB);

                // Normalize for D65 white point
                float3 refWhite = float3(0.9505, 1.0000, 1.0890);
                float3 ratio = xyz / refWhite;

                float fx = f(ratio.x);
                float fy = f(ratio.y);
                float fz = f(ratio.z);

                float L = (ratio.y > 0.008856) ? (116.0 * fy - 16.0) : (903.3 * ratio.y);
                float a = 500.0 * (fx - fy);
                float b = 200.0 * (fy - fz);

                return float3(L, a, b);
            }

            float3 CIELABtoRGB(float3 lab)
            {
                float L = lab.x;
                float a = lab.y;
                float b = lab.z;

                float fy = (L + 16.0) / 116.0;
                float fx = a / 500.0 + fy;
                float fz = fy - b / 200.0;

                float x = (pow(fx, 3.0) > 0.008856) ? pow(fx, 3.0) : (fx - 16.0 / 116.0) / 7.787;
                float y = (L > 7.9996) ? pow((L + 16.0) / 116.0, 3.0) : L / 903.3;
                float z = (pow(fz, 3.0) > 0.008856) ? pow(fz, 3.0) : (fz - 16.0 / 116.0) / 7.787;

                // Convert back to XYZ
                float3 refWhite = float3(0.9505, 1.0000, 1.0890);
                float3 xyz = float3(x, y, z) * refWhite;

                // XYZ to Linear RGB
                const float3x3 XYZtoRGB = float3x3(
                    3.2406, -1.5372, -0.4986,
                    -0.9689, 1.8758, 0.0415,
                    0.0557, -0.2040, 1.0570
                );
                float3 linearRGB = mul(XYZtoRGB, xyz);

                // Linear RGB to sRGB
                float3 srgb = saturate(pow(abs(linearRGB), 1.0 / 2.2));

                return srgb;
            }


            // get the point on the circle around _Target, defined by the radius _FoveaSize
            float GetMaxDistance(float2 uv, float maxDist)
            {
                float2 C = _Target;
                float2 Rpoint = _Center;
                float2 Rdir = uv - _Center;
                Rdir = normalize(Rdir);
                float2 U = C - Rpoint;
                float2 U1 = dot(U, Rdir) * Rdir;
                float2 U2 = U - U1;
                float d = length(U2);
                if (d > _FoveaSize)
                {
                    return maxDist;
                }
                float m = sqrt(_FoveaSize * _FoveaSize - d * d);
                float2 P1 = Rpoint + U1 + m * Rdir;
                float2 P2 = Rpoint + U1 - m * Rdir;
                float P1dist = length(P1 - _Center);
                float P2dist = length(P2 - _Center);
                return P1dist < P2dist ? P1dist : P2dist;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 centeredUV = uv - _Center;
                centeredUV.y *= _ScreenParams.y / _ScreenParams.x;
                float dist = length(centeredUV);
                float2 centeredTarget = _Target - _Center;
                float maxDist = length(centeredTarget);
                int eye = _Target.x < _Center.x ? 0:1;

                if (_OneEye > 0 && unity_StereoEyeIndex == eye)
                {
                    return tex2D(_MainTex, uv);
                }

                if (dist > maxDist || dist < 0.02f)
                {
                    return tex2D(_MainTex, uv);
                }

                maxDist = GetMaxDistance(uv, maxDist);

                float2 direction = _Target - _Center;
                float dirLenSq = dot(direction, direction);
                direction = dirLenSq > 1e-8 ? normalize(direction) : float2(1, 0);

                float2 toUV = uv - _Center;
                float toUVLenSq = dot(toUV, toUV);
                toUV = toUVLenSq > 1e-8 ? normalize(toUV) : float2(1, 0);


                float angleDot = dot(toUV, direction);
                angleDot = clamp(angleDot, -1.0, 1.0);

                float cosAngle = cos(_Angle);
                if (angleDot < cosAngle)
                {
                    return tex2D(_MainTex, uv);
                }

                // ripple
                float invAr = _ScreenParams.y / _ScreenParams.x;
                float x = (_Center.x - uv.x);
                float y = (_Center.y - uv.y) * invAr;
                // frequency distance stays the same
                float r = -(sqrt(x * x + y * y));
                float timeFactor = _Time.y * _Speed;
                float rippleEffect;
                if(_StereoInverse == 1 && unity_StereoEyeIndex == 1)
                {
                    rippleEffect = _Brightness + _Intensity * sin((r - timeFactor + (_Frequency * 3.14159)) / _Frequency);
                }else
                {
                    rippleEffect = _Brightness + _Intensity * sin((r - timeFactor) / _Frequency);
                }

                float angle = acos(angleDot);
                float angleInside = _Angle - angle;
                float threshold = _Angle * _FallOff;
                float normalizedFalloff = saturate((angleInside - threshold) / threshold);
                float angleFalloff = pow(normalizedFalloff, _FallOff);

                // CIELab
                float4 texColor = tex2D(_MainTex, uv);
                float t = saturate(dist / maxDist);
                float falloff = pow(1.0 - t, _FallOff);
                falloff *= angleFalloff;
                rippleEffect = lerp(1.0, rippleEffect, falloff);
                float4 baseColor = texColor;
                float3 lab = RGBtoCIELAB(baseColor.rgb);
                lab.x *= rippleEffect;
                float3 adjustedRGB = CIELABtoRGB(lab);
                return float4(adjustedRGB, baseColor.a);
            }

            ENDCG
        }
    }
}