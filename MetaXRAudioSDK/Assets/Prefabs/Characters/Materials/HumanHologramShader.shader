Shader "Unlit/HumanShader"
{
    Properties
    {
        _ndnMinMax("Normal dot normal gradient", Vector) = (0.0, 0.0, 0.0, 0.0)
        _fresnelPower("Fresnel power", float) = 1.0
        _fresnelMinMax("Fresnel Min Max", Vector) = (0.0, 1.0, 0.0, 0.0)
        _triLineBlur ("Triangle line blur", Range(0.0, 1.0)) = 0.0
        _triLineWidth ("Triangle line width", Range(0.0, 0.3)) = 0.0
        _colorMin ("Color Min", Color) = (0.0,0.0,1.0,1.0)
        _colorMax ("Color Max", Color) = (1.0,1.0,1.0,1.0)
        _sphereNormal ("Sphere normal from blender", Vector) = (0.066667, 0.500000, 0.863456, 0.0)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 localNormal : TEXCOORD2;
                float3 worldNormal : NORMAL;
                float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 _ndnMinMax;
            float _triLineWidth;
            float _triLineBlur;
            float3 _sphereNormal;

            float3 _colorMin;
            float3 _colorMax;
            float _fresnelPower;
            float2 _fresnelMinMax;

            float fresnelEffect(float3 Normal, float3 ViewDir, float Power)
            {
                return pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
            }

            float inverseLerp(float t, float a, float b) {
                return (t - a) / (b - a);
            }

            float overlay(float a, float b) {
                if (a < 0.5) {
                    return 2 * a * b;
                }
                else {
                    return 1.0 - 2.0 * (1.0 - a) * (1.0 - b);
                }
            }

            float triangleSDF(float2 uv) {
                float d1 = 1.0 - uv.x;
                float d2 = uv.y;

                float d4 = uv.x;
                float d5 = 1.0 - uv.y;

                float2 norm1 = normalize(float2(1.0, 1.0));
                float d3 = length((dot(norm1, uv) * norm1) - uv);

                float minSD1 = min(d1, d2);
                float minSD2 = min(d4, d5);
                float minS = min(minSD1, minSD2);

                return min(minS, d3);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.localNormal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 viewVec = UnityWorldSpaceViewDir(i.worldPos);
                float3 viewDir = normalize(viewVec);

                float ndn = dot(_sphereNormal, i.worldNormal);

                float ndnRemap = saturate(inverseLerp(ndn, _ndnMinMax.x, _ndnMinMax.y));

                float fresnel1 = fresnelEffect(i.worldNormal, viewDir, _fresnelPower);
                fresnel1 = saturate(inverseLerp(fresnel1, _fresnelMinMax.x, _fresnelMinMax.y));

                float out1 = ndnRemap + fresnel1;

                //Better aliased lines
                //float triLine = triangleSDF(i.uv) - _triLineWidth;
                //triLine = triLine / (length(float2(ddx(triLine), ddy(triLine))));
                //triLine = saturate(triLine);

                float triLine = saturate(inverseLerp(triangleSDF(i.uv), 0.0, _triLineWidth));
                triLine = saturate(inverseLerp(triLine, 1.0 - _triLineBlur, 1.0));

                float out2 = saturate(overlay(out1, 1.0 - triLine));

                float3 color = lerp(_colorMin, _colorMax, out2);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
