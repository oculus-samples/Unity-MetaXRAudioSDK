/************************************************************************************
Copyright (c) Meta Platforms, Inc. and affiliates.
All rights reserved.

Licensed under the Oculus SDK License Agreement (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
************************************************************************************/

Shader "Oculus/OpaqueBakedGI"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}
        _DimColor("Dim Color", Color) = (1,1,1,1)
        _DimBlend("Dim Blend", Range(0,1)) = 0
        _Tiling("Tiling", float) = 1
        //_EnvironmentLight("Environment Light", Color) = (1,1,1,1)
        //_EnvironmentLightIntensity("Environment Light Intensity", Range(0,1)) = 0
        _ShadowDarkColor("Shadow Dark Color", Color) = (1,1,1,1)
        _ShadowBrightColor("Shadow Bright Color", Color) = (1,1,1,1)
        //_TransitionRange("Transition Range", Vector) = (0,1,0,0)
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
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 lightmapUV: TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _DimColor;
            float _DimBlend;
            float _Tiling;
            //fixed3 _EnvironmentLight;
            //float _EnvironmentLightIntensity;
            fixed3 _ShadowDarkColor;
            fixed3 _ShadowBrightColor;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightmapUV = v.lightmapUV;
                return o;
            }

            float inverseLerp(float t, float a, float b) {
                return (t - a) / (b - a);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //Get Baked GI
                float2 lightmapUV = i.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
                half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV);
                half3 bakedColor = DecodeLightmap(bakedColorTex);

                float2 uv = i.uv * _Tiling;
                fixed4 c = tex2D(_MainTex, uv);
                float3 albedo = lerp(c.rgb, _DimColor, _DimBlend);

                //float3 light = bakedColor + _EnvironmentLight * _EnvironmentLightIntensity;
                float3 light = lerp(_ShadowDarkColor, _ShadowBrightColor, bakedColor);
                light = saturate(light);
                float4 color = float4(albedo * light, 1.0);

                return color;
            }
            ENDCG
        }
    }
}
