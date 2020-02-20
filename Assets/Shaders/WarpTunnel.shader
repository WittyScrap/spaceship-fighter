Shader "Unlit/WarpTunnel"
{
    Properties
    {
		    _MainTex		("Texture", 2D)						 = "white" {}
			_Displacement	("Displacement", Range(-1.0, 1.0))	 = 1.0

			_Highlight		("Highlights", Color)				 = (1, 1, 1, 0)
			_Peaks			("Peaks", Color)					 = (0, 0, 0.5, 0)

			_Detail			("Detail", Range(1, 32))			 = 4
			_PeakAmount		("Peak amount", Range(0, 1))		 = 0.5

			_Speed			("Scroll speed", Float)				 = 1
			_Cutoff			("Cutoff", Range(0, 2))				 = 0
			_Torque			("Torque", Float)					 = 1

			_Power			("Power", Float)					 = 1.5
			_PeakHeight		("Peak height", Float)				 = 10
    }
    SubShader 
	{
        Tags { "RenderType"="Opaque" }
        LOD 300
            
        CGPROGRAM
        #pragma surface surf SimpleLambert vertex:disp tessellate:tessFixed nolightmap
        #pragma target 5.0


		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
			return half4(s.Albedo.rgb, 1);
		}

		float _Detail;

		float4 tessFixed()
		{
			return _Detail;
		}

        struct appdata 
		{
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float2 texcoord : TEXCOORD0;
        };

		sampler2D _MainTex;
		float _Displacement;
		float _Speed;
		float _PeakHeight;

        void disp (inout appdata v)
        {
            float d = tex2Dlod(_MainTex, float4(v.texcoord.xy + _Time.xy * _Speed % 1.f,0,0)).r * _Displacement * _PeakHeight;
            v.vertex.xyz += v.normal * d;
        }

        struct Input 
		{
            float2 uv_MainTex;
        };

		float4 _Highlight;
		float4 _Peaks;
		float  _PeakAmount;
		float  _Cutoff;
		float  _Power;
		float  _Torque;

        void surf (Input IN, inout SurfaceOutput o) 
		{
			float y_scroll = _Time.x * _Speed % 2.f;
			float x_scroll = _Time.x * _Torque % 2.f;

			float2 sourceUV = IN.uv_MainTex;

			float u = (sourceUV.x + x_scroll);
			float v = (sourceUV.y + y_scroll);

			float2 uv = float2(u, v);

            half4 map = saturate(tex2D (_MainTex, uv) * _Power);
			clip((1 - _Cutoff) - 1);

			map = saturate(map + _PeakAmount);
			half4 col = lerp(saturate(_Highlight), saturate(_Peaks), map);

            o.Albedo = col.rgb;
        }
        ENDCG
    }
}
