Shader "Custom/SWAVE_backup"
{
    Properties
    {
        _RippleColor ("Ripple Color", Color) = (1, 1, 1, 1)
        _WorldRippleCenter ("Ripple Center (World)", Vector) = (0, 0, 0, 0)
        _Frequency ("Ripple Frequency", Float) = 10
        _Speed ("Ripple Speed", Float) = 4
        _Intensity ("Ripple Intensity", Float) = 1
        _Width ("Ripple Width", Float) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _RippleColor;
            float4 _WorldRippleCenter;
            float _Frequency;
            float _Speed;
            float _Intensity;
            float _Width;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = distance(i.worldPos, _WorldRippleCenter.xyz);
                float time = _Time.y * _Speed;
                float wave = sin(dist * _Frequency + time);

                float band = smoothstep(_Width, 0.0, abs(wave));
                float brightness = band * _Intensity;
                float alpha = brightness * _RippleColor.a;

                return float4(_RippleColor.rgb * brightness, alpha);
            }

            ENDCG
        }
    }
}
