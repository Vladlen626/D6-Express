Shader "Game/StylizedOpaqueSimple"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2

        _ShadowColor ("Shadow Color", Color) = (0.74, 0.78, 0.87, 1)
        _LightWrap ("Light Wrap", Range(0.0, 1.0)) = 0.2
        _RampSoftness ("Ramp Softness", Range(0.001, 1.0)) = 0.3
        _AmbientStrength ("Ambient Strength", Range(0.0, 1.0)) = 0.2
        _TextureInfluence ("Texture Influence", Range(0.0, 1.0)) = 1.0
        _SaturationBoost ("Saturation Boost", Range(0.5, 1.5)) = 1.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma prefer_hlslcc gles
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uvLM : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float2 lightmapUV : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float _Cull;
                float _LightWrap;
                float _RampSoftness;
                float _AmbientStrength;
                float _TextureInfluence;
                float _SaturationBoost;
            CBUFFER_END

            half ComputeStylizedRamp(half ndotl, half lightWrap, half rampSoftness)
            {
                half wrapped = saturate((ndotl + lightWrap) / (1.0h + lightWrap));
                half halfSoftness = max(rampSoftness * 0.5h, 0.0005h);
                return smoothstep(0.5h - halfSoftness, 0.5h + halfSoftness, wrapped);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #if defined(LIGHTMAP_ON)
                    output.lightmapUV = input.uvLM * unity_LightmapST.xy + unity_LightmapST.zw;
                #else
                    output.lightmapUV = float2(0.0, 0.0);
                #endif
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE frontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                normalWS *= IS_FRONT_VFACE(frontFace, 1.0h, -1.0h);
                half3 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;

                half textureInfluence = saturate((half)_TextureInfluence);
                half3 baseAlbedo = lerp(half3(1.0h, 1.0h, 1.0h), baseTex, textureInfluence) * (half3)_BaseColor.rgb;

                Light mainLight = GetMainLight();
                half3 lightDirWS = normalize(mainLight.direction);
                half ramp = ComputeStylizedRamp(dot(normalWS, lightDirWS), saturate((half)_LightWrap), saturate((half)_RampSoftness));

                half3 litColor = baseAlbedo * (half3)mainLight.color;
                half3 shadowColor = baseAlbedo * (half3)_ShadowColor.rgb;
                half3 color = lerp(shadowColor, litColor, ramp);

                half3 bakedGI = SAMPLE_GI(input.lightmapUV, SampleSH(normalWS), normalWS);
                color += baseAlbedo * bakedGI * saturate((half)_AmbientStrength);

                half saturationBoost = clamp((half)_SaturationBoost, 0.5h, 1.5h);
                half luminance = dot(color, half3(0.299h, 0.587h, 0.114h));
                color = lerp(luminance.xxx, color, saturationBoost);

                return half4(saturate(color), 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma prefer_hlslcc gles
            #pragma vertex StylizedMetaVertex
            #pragma fragment StylizedMetaFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float _Cull;
                float _LightWrap;
                float _RampSoftness;
                float _AmbientStrength;
                float _TextureInfluence;
                float _SaturationBoost;
            CBUFFER_END

            Varyings StylizedMetaVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2, unity_LightmapST, unity_DynamicLightmapST);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 StylizedMetaFragment(Varyings input) : SV_Target
            {
                half3 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                half textureInfluence = saturate((half)_TextureInfluence);
                half3 baseAlbedo = lerp(half3(1.0h, 1.0h, 1.0h), baseTex, textureInfluence) * (half3)_BaseColor.rgb;

                half saturationBoost = clamp((half)_SaturationBoost, 0.5h, 1.5h);
                half luminance = dot(baseAlbedo, half3(0.299h, 0.587h, 0.114h));
                baseAlbedo = lerp(luminance.xxx, baseAlbedo, saturationBoost);

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = saturate(baseAlbedo);
                metaInput.Emission = half3(0.0h, 0.0h, 0.0h);
                return UnityMetaFragment(metaInput);
            }
            ENDHLSL
        }
    }
}
