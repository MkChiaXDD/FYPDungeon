Shader "Universal Render Pipeline/Particles/DissolveWithOffset_MainTexOnly"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _DissolveTex("Dissolve Texture", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _EdgeWidth("Edge Width", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType" = "Plane"
            "IgnoreProjector" = "True"
        }
        
        LOD 300
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "URP Dissolve Particle"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float4 customData   : TEXCOORD1;  // Particle custom stream
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float dissolveAmount: TEXCOORD1;
                float2 mainTexOffset: TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DissolveTex);
            SAMPLER(sampler_DissolveTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EdgeColor;
                float _EdgeWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                // URP transformation function
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                OUT.dissolveAmount = IN.customData.x;           // Dissolve progress
                OUT.mainTexOffset = float2(IN.customData.yz);   // UV offset
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                // Apply UV offset to main texture
                float2 offsetUV = IN.uv + IN.mainTexOffset;
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, offsetUV);
                
                // Early discard for transparent pixels
                if (mainTex.a <= 0.001) discard;
                
                // Sample dissolve texture (original UVs)
                half dissolveValue = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, IN.uv).r;
                
                // Dissolve clipping
                if (dissolveValue < IN.dissolveAmount) discard;
                
                // Edge glow effect
                if (dissolveValue < (IN.dissolveAmount + _EdgeWidth)) {
                    return _EdgeColor;
                }
                
                // Final color with particle system modulation
                return mainTex * IN.color;
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Particles/Unlit"
}