Shader "Custom/URP_ASCII_Font"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _FontTex ("Font Atlas", 2D) = "white" {}

        _CellSize ("ASCII Cell Size", Range(8,64)) = 24
        _CellAspect ("Cell Aspect XY", Vector) = (1,1,0,0)

        _Contrast ("Contrast", Range(0.2,3)) = 1
        _Brightness ("Brightness", Range(-1,1)) = 0

        _Tint ("ASCII Tint", Color) = (1,1,1,1)
        _Background ("Background", Color) = (0,0,0,1)

        _AtlasGrid ("Atlas Columns/Rows", Vector) = (16,16,0,0)
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
            Name "ASCII_Font"

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_FontTex);
            SAMPLER(sampler_FontTex);

            float _CellSize;
            float4 _CellAspect;
            float _Contrast;
            float _Brightness;
            float4 _Tint;
            float4 _Background;
            float4 _AtlasGrid;

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

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                float2 pix = i.uv * resolution;

                // cell scaling
                float2 cellSize = max(_CellSize,1) * float2(_CellAspect.x,_CellAspect.y);
                float2 snappedUV = floor(pix/cellSize) * cellSize / resolution;

                // sample screen color
                float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, snappedUV).rgb;

                // grayscale
                float gray = dot(col, float3(0.299,0.587,0.114));
                gray = saturate(gray * _Contrast + _Brightness);

                // map brightness to ASCII index
                float totalChars = _AtlasGrid.x * _AtlasGrid.y;
                float charIndex = floor(gray * (totalChars-1));

                // calculate grid position
                float2 atlasDim = _AtlasGrid.xy;
                float2 charPos;
                charPos.x = fmod(charIndex, atlasDim.x);
                charPos.y = floor(charIndex / atlasDim.x);

                // local UV inside cell
                float2 glyphUV = frac(pix/cellSize);

                // sample font atlas
                float2 fontUV = (charPos + glyphUV) / atlasDim;
                float glyph = SAMPLE_TEXTURE2D(_FontTex, sampler_FontTex, fontUV).r;

                // final color
                float3 final = lerp(_Background.rgb, col * _Tint.rgb, glyph);

                return float4(final,1);
            }

            ENDHLSL
        }
    }
}
