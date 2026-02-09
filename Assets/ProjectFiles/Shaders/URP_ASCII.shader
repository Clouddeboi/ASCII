Shader "Custom/URP_ASCII"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _CellSize ("Cell Size", Float) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "ASCII"

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _CellSize;
            //float4 _ScreenParams;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float character(int n, float2 p)
            {
                p = floor(p * float2(-4,4) + 2.5);

                if (p.x < 0 || p.x > 4 || p.y < 0 || p.y > 4)
                    return 0;

                int a = (int)p.x + 5 * (int)p.y;
                return ((n >> a) & 1) ? 1 : 0;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 pix = i.uv * _ScreenParams.xy;

                float2 snappedUV =
                    floor(pix/_CellSize)*_CellSize/_ScreenParams.xy;

                float3 col =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snappedUV).rgb;

                float gray = dot(col, float3(0.3,0.59,0.11));

                int n = 4096;
                if(gray>0.2) n=65600;
                if(gray>0.3) n=163153;
                if(gray>0.4) n=15255086;
                if(gray>0.5) n=13121101;
                if(gray>0.6) n=15252014;
                if(gray>0.7) n=13195790;
                if(gray>0.8) n=11512810;

                float2 p = fmod(pix/4.0,2.0)-1.0;

                float ascii = character(n,p);

                return float4(col * ascii, 1);
            }

            ENDHLSL
        }
    }
}
