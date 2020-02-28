// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/RingSystem"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturation", Range(0, 1)) = .5
        _Shininess ("Shininess", Float) = 10
    }
    SubShader 
    {
        Pass 
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #pragma multi_compile_fwdbase

            #include "AutoLight.cginc"


            struct v2f
            {
                float4 pos          : SV_POSITION;
                LIGHTING_COORDS(0,1)
                float2 uv_MainTex   : TEXCOORD2;
                float3 normal       : TEXCOORD3;
                float4 worldPos     : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Saturation;
            float _Shininess;

            v2f vert(appdata_base v) 
            {
                v2f o;
                o.pos           = UnityObjectToClipPos (v.vertex);
                o.worldPos      = mul (unity_ObjectToWorld, v.vertex);
                o.uv_MainTex    = TRANSFORM_TEX (v.texcoord, _MainTex);
                o.normal        = UnityObjectToWorldNormal (v.normal);

                TRANSFER_VERTEX_TO_FRAGMENT (o);

                return o;
            }

            float3 get_view(float4 worldPos)
            {
                return normalize (_WorldSpaceCameraPos - worldPos.xyz);
            }

            float light_dir()
            {
                return normalize (_WorldSpaceLightPos0.xyz);
            }

            float specular(float3 normal, float3 view)
            {
                float3 lightDir  = light_dir();
                float3 reflected = reflect (-lightDir, normalize(normal));
                float  ldv       = dot (reflected, view);
                float  value     = max (0.f, ldv);

                return pow (value, _Shininess) * (dot(normal, lightDir) >= 0);
            }

            float greyscale(float4 col)
            {
                return dot(col.rgb, fixed3(.222, .707, .071));
            }

            fixed4 frag(v2f i) : COLOR
            {
                float4 col          = tex2D (_MainTex, i.uv_MainTex);
                float attenuation   = LIGHT_ATTENUATION (i);
                float value         = col;
                clip(value - .01f);

                float specularValue   = specular (i.normal, get_view(i.worldPos));
                float ndotl           = dot(i.normal, -light_dir());

                float4 saturated      = lerp(greyscale(col), col, _Saturation) * _Saturation;
                float4 withSpecular   = saturated + specularValue;
                float4 litOutput      = withSpecular * attenuation;

                float3 L              = -light_dir();
                float3 V              = get_view(i.worldPos);
                float3 N              = i.normal;

                float3 H              = normalize(L + N);
                float VdotH           = pow(saturate(dot(V, -H)), _Shininess);
                float3 I              = VdotH + unity_AmbientSky;

                float4 unlitOutput    = float4(litOutput + I, litOutput.a) * attenuation * .15f;

                return lerp(unlitOutput, litOutput, max(0.0f, ndotl));
            }

            ENDCG
        }
    }
    
    Fallback "VertexLit"
}
