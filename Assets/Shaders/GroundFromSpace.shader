// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Atmosphere/GroundFromSpace" 
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_SeaLevel ("Sea level", Range(0, 1)) = 0.5
		_SeaColor ("Sea colour", Color) = (0, 0.2, 0.95)
		_LandColor ("Land colour", Color) = (0.5, 0.68, 0.5)
		_Mountain ("Mountain colour", Color) = (0, 0, 0, 0)

		_Seed ("Seed", Vector) = (0, 0, 0, 0)
		_NoiseScaleA ("Noise scale 1", Float) = 2
		_NoiseScaleB ("Noise scale 2", Float) = 10
		_NoiseScaleC ("Noise scale 3", Float) = 100
		_NoiseScaleD ("Noise scale 4", Float) = 100
		_NoiseScaleE ("Noise scale 5", Float) = 100

		_Shininess ("Water reflectivity", Float) = 10

		[Toggle] _DebugView ("Debug", Int) = 0
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
    	Pass 
    	{
    		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			sampler2D _MainTex;
			
			uniform float3 v3Translate;		// The objects world pos
			uniform float3 v3LightPos;		// The direction vector to the light source
			uniform float3 v3InvWavelength; // 1 / pow(wavelength, 4) for the red, green, and blue channels
			uniform float fOuterRadius;		// The outer (atmosphere) radius
			uniform float fOuterRadius2;	// fOuterRadius^2
			uniform float fInnerRadius;		// The inner (planetary) radius
			uniform float fInnerRadius2;	// fInnerRadius^2
			uniform float fKrESun;			// Kr * ESun
			uniform float fKmESun;			// Km * ESun
			uniform float fKr4PI;			// Kr * 4 * PI
			uniform float fKm4PI;			// Km * 4 * PI
			uniform float fScale;			// 1 / (fOuterRadius - fInnerRadius)
			uniform float fScaleDepth;		// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
			uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
			uniform float fHdrExposure;		// HDR exposure
		
			struct v2f 
			{
    			float4 pos		: SV_POSITION;
				float3 normal	: NORMAL;
				float4 raw_pos	: TEXCOORD0;
    			float2 uv		: TEXCOORD1;
				float3 normalDir: TEXCOORD2;
				float4 worldPos : TEXCOORD3;
    			float3 c0		: COLOR0;
    			float3 c1		: COLOR1;
			};
			
			float scale(float fCos)
			{
				float x = 1.0 - fCos;
				return fScaleDepth * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
			}

			v2f vert(appdata_full v)
			{
			    float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				float fCameraHeight2 = fCameraHeight*fCameraHeight;			// fCameraHeight^2
				
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(unity_ObjectToWorld, v.vertex).xyz - v3Translate;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the closest intersection of the ray with the outer atmosphere (which is the near point of the ray passing through the atmosphere)
				float B = 2.0 * dot(v3CameraPos, v3Ray);
				float C = fCameraHeight2 - fOuterRadius2;
				float fDet = max(0.0, B*B - 4.0 * C);
				float fNear = 0.5 * (-B - sqrt(fDet));
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos + v3Ray * fNear;
				fFar -= fNear;
				float fDepth = exp((fInnerRadius - fOuterRadius) / fScaleDepth);
				float fCameraAngle = dot(-v3Ray, v3Pos) / length(v3Pos);
				float fLightAngle = dot(v3LightPos, v3Pos) / length(v3Pos);
				float fCameraScale = scale(fCameraAngle);
				float fLightScale = scale(fLightAngle);
				float fCameraOffset = fDepth*fCameraScale;
				float fTemp = (fLightScale + fCameraScale);
				
				const float fSamples = 2.0;
				
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				float3 v3Attenuate;
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fScatter = fDepth*fTemp - fCameraOffset;
					v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
				
    			v2f OUT;
				OUT.raw_pos = v.vertex;
				OUT.normal = v.normal;
    			OUT.pos = UnityObjectToClipPos(v.vertex);
    			OUT.uv = v.texcoord.xy;
    			OUT.c0 = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
    			OUT.c1 = v3Attenuate;
				OUT.worldPos = mul(unity_ObjectToWorld, v.vertex);
				OUT.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
    			
    			return OUT;
			}

			#include "PortalingPerlin.cginc"

			float _SeaLevel;
			float4 _SeaColor;
			float4 _LandColor;
			float4 _Mountain;
			float4 _Seed;

			float _NoiseScaleA;
			float _NoiseScaleB;
			float _NoiseScaleC;
			float _NoiseScaleD;
			float _NoiseScaleE;

			float _Shininess;

			half3 texel(v2f IN)
			{
				float3 position = IN.normal;
				position += _Seed.xyz;

				float seedStarter = saturate(perlin3D(position * _NoiseScaleA) - perlin3D(position * _NoiseScaleB) + perlin3D(position * _NoiseScaleC));
				float landValue = perlin(half2(seedStarter * _NoiseScaleA, seedStarter + _Seed.x * _NoiseScaleA)) - perlin3D(position * _NoiseScaleD) + perlin3D(position * _NoiseScaleE);

				float3 normal = normalize(IN.normalDir);
				float3 view = normalize(_WorldSpaceCameraPos - IN.worldPos.xyz);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				float3 specularReflection = pow(
												max(
													0.0,
													dot(
														reflect(-lightDir, normal),
														view
													)
												),
												_Shininess
											);

				specularReflection *= dot(normal, lightDir) >= 0.0;
				float isWater = landValue < _SeaLevel;

				return lerp(lerp(_LandColor, _Mountain, seedStarter), _SeaColor + specularReflection, isWater);
			}

			int _DebugView;
			
			half4 frag(v2f IN) : COLOR
			{
				half3 tex = texel(IN);

				float3 col = IN.c0 + 0.25 * IN.c1;

				//Adjust color from HDR
				col = 1.0 - exp(col * -fHdrExposure);
				tex *= col.b;

				return half4(tex + col, 1.0);
			}
			
			ENDCG

    	}
	}
}