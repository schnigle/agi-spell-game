Shader "Refract"
{
	Properties
	{
		_MainTex ("Distortion texture", 2D) = "white" {}
        _Strength ("Geometry distortion strength", Range(0,1)) = 0.5
        _MapStrength ("Normal map distortion strength", Range(0,1)) = 0.5
        _Tint ("Tint", Color) = (1,1,1,1)
	}
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        GrabPass
        {
            "_RefractBackground"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float _Strength;
            float _MapStrength;
            float4 _Tint;

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD1;
            };

            v2f vert(appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
				o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            sampler2D _RefractBackground;

            half4 frag(v2f i) : SV_Target
            {
				float4 uv = i.grabPos;
                float3 normal = i.normal * 0.1 * _Strength;
				i.uv.x += sin(_Time.x*0.4) * 0.15;
				i.uv.y += sin(_Time.x*0.4) * 0.15;
                normal += UnpackNormal(tex2D(_MainTex, i.uv)) * _MapStrength;
                float2 screenPos = uv.xy / uv.w;
                screenPos += normal;
                half4 bgcolor = tex2D(_RefractBackground, screenPos);
                return bgcolor * _Tint + _Tint.a * _Tint;
            }
            ENDCG
        }

    }
}