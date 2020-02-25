Shader "Unlit/PlanetRings"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Shininess ("Shininess", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags {"LightMode" = "ForwardBase" }

            Cull Off
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float3 worldPos : TEXCOORD2;
                float3 normalDir : TEXCOORD3;
                float3 normal : TEXCOORD4;
                LIGHTING_COORDS(5, 6)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Shininess;

            v2f vert (appdata v)
            {
                v2f o;

                v.normal = float3(0, 1, 0); // Always up

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.normal = v.normal;

                UNITY_TRANSFER_FOG(o,o.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed lighting (v2f i)
            {
                float attenuation = LIGHT_ATTENUATION(i);
                return dot(i.normalDir, normalize(-_WorldSpaceLightPos0.xyz)) * attenuation;
            }

            fixed specular (v2f i)
            {
                float3 normal = normalize(i.normalDir);
                float3 view = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
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
                return specularReflection;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed light = lighting(i) + specular(i);
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col * light;
            }

            ENDCG
        }
    }
}
