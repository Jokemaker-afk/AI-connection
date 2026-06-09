Shader "Universal Render Pipeline/ShieldBubbleFresnel"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.78, 0.92, 1, 0.08)
        _EdgeColor ("Edge Color", Color) = (0.88, 0.95, 1, 0.42)
        _RimTint ("Rim Tint", Color) = (0.72, 0.82, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 2.2
        _EdgeBoost ("Edge Boost", Range(0, 3)) = 1.15
        _EmissionStrength ("Emission", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ShieldBubbleFresnel"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EdgeColor;
                half4 _RimTint;
                half _FresnelPower;
                half _EdgeBoost;
                half _EmissionStrength;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);
                half ndv = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0h - ndv, _FresnelPower);
                half edgeMix = saturate(fresnel * _EdgeBoost);
                half4 color = lerp(_BaseColor, _EdgeColor, edgeMix);
                color.rgb += _RimTint.rgb * fresnel * _EmissionStrength;
                color.a = lerp(_BaseColor.a, _EdgeColor.a, edgeMix);
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
