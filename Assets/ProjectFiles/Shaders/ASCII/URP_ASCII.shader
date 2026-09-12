//Procedural bitmap-font ASCII shader for URP.
//Ported from movAX13h's "Bitmap to ASCII" ShaderToy shader (Sept 2013, updated 2023).
//Link: https://www.shadertoy.com/view/lssGDj
//Each glyph is a 5x5 bit pattern packed into a single int, no texture atlas,
//so there's no grid/UV bleeding to get wrong.
Shader "Custom/URP_ASCII_Procedural"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}

        _CellSize ("ASCII Cell Size (px)", Range(4,32)) = 8

        _Contrast ("Scene Contrast", Range(0.2,3)) = 1
        _Brightness ("Scene Brightness", Range(-1,1)) = 0

        _Tint ("ASCII Tint", Color) = (1,1,1,1)
        _Background ("Background", Color) = (0,0,0,1)
        _BackgroundBlend ("Background Blend (0 = flat bg, 1 = scene bleeds through)", Range(0,1)) = 0

        [Toggle] _FullCharset ("Use Full Character Set (A-Z, 0-9)", Float) = 1
        [Toggle] _BWMode ("Black & White Mode (ignore scene color)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "ASCII_Procedural"
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _CellSize;
            float _Contrast;
            float _Brightness;
            float4 _Tint;
            float4 _Background;
            float _BackgroundBlend;
            float _FullCharset;
            float _BWMode;

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

            //Decodes bit `a` of packed glyph integer `n` for local cell
            //coordinate p (expected roughly in [-1,1] before the internal scale).
            float Character(int n, float2 p)
            {
                p = floor(p * float2(-4.0, 4.0) + 2.5);
                if (clamp(p.x, 0.0, 4.0) == p.x)
                {
                    if (clamp(p.y, 0.0, 4.0) == p.y)
                    {
                        int a = int(round(p.x) + 5.0 * round(p.y));
                        if (((n >> a) & 1) == 1) return 1.0;
                    }
                }
                return 0.0;
            }

            //Limited 8-step set: space : * o & 8 @ #
            int PickCharLimited(float gray)
            {
                int n = 4096;
                if (gray > 0.2) n = 65600;    // :
                if (gray > 0.3) n = 163153;   // *
                if (gray > 0.4) n = 15255086; // o
                if (gray > 0.5) n = 13121101; // &
                if (gray > 0.6) n = 15252014; // 8
                if (gray > 0.7) n = 13195790; // @
                if (gray > 0.8) n = 11512810; // #
                return n;
            }

            //Full 40-step set including A-Z and 0-9 for smoother gradients.
            int PickCharFull(float gray)
            {
                int n = 4096;
                if (gray > 0.0233) n = 4096;
                if (gray > 0.0465) n = 131200;
                if (gray > 0.0698) n = 4329476;
                if (gray > 0.0930) n = 459200;
                if (gray > 0.1163) n = 4591748;
                if (gray > 0.1395) n = 12652620;
                if (gray > 0.1628) n = 14749828;
                if (gray > 0.1860) n = 18393220;
                if (gray > 0.2093) n = 15239300;
                if (gray > 0.2326) n = 17318431;
                if (gray > 0.2558) n = 32641156;
                if (gray > 0.2791) n = 18393412;
                if (gray > 0.3023) n = 18157905;
                if (gray > 0.3256) n = 17463428;
                if (gray > 0.3488) n = 14954572;
                if (gray > 0.3721) n = 13177118;
                if (gray > 0.3953) n = 6566222;
                if (gray > 0.4186) n = 16269839;
                if (gray > 0.4419) n = 18444881;
                if (gray > 0.4651) n = 18400814;
                if (gray > 0.4884) n = 33061392;
                if (gray > 0.5116) n = 15255086;
                if (gray > 0.5349) n = 32045584;
                if (gray > 0.5581) n = 18405034;
                if (gray > 0.5814) n = 15022158;
                if (gray > 0.6047) n = 15018318;
                if (gray > 0.6279) n = 16272942;
                if (gray > 0.6512) n = 18415153;
                if (gray > 0.6744) n = 32641183;
                if (gray > 0.6977) n = 32540207;
                if (gray > 0.7209) n = 18732593;
                if (gray > 0.7442) n = 18667121;
                if (gray > 0.7674) n = 16267326;
                if (gray > 0.7907) n = 32575775;
                if (gray > 0.8140) n = 15022414;
                if (gray > 0.8372) n = 15255537;
                if (gray > 0.8605) n = 32032318;
                if (gray > 0.8837) n = 32045617;
                if (gray > 0.9070) n = 33081316;
                if (gray > 0.9302) n = 32045630;
                if (gray > 0.9535) n = 33061407;
                if (gray > 0.9767) n = 11512810;
                return n;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 resolution = _ScreenParams.xy;
                float2 pix = i.uv * resolution;

                float cellSize = max(_CellSize, 1.0);

                //Sample one color per 8px-equivalent block (scaled by _CellSize).
                float2 blockUV = floor(pix / cellSize) * cellSize / resolution;
                float3 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, blockUV).rgb;

                float gray = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                gray = saturate(gray * _Contrast + _Brightness);

                int n = (_FullCharset > 0.5) ? PickCharFull(gray) : PickCharLimited(gray);

                //Local coordinate within the glyph cell, matching the
                //original shader's /4.0 internal scale (cellSize acts as
                //the outer 8px block; the /4 controls glyph density inside it).
                float2 p = fmod(pix / (cellSize * 0.5), 2.0) - 1.0;

                float glyph = Character(n, p);

                float3 asciiColor = (_BWMode > 0.5) ? glyph.xxx : (col * glyph * _Tint.rgb);
                float3 bg = lerp(_Background.rgb, col, _BackgroundBlend);
                float3 final = lerp(bg, asciiColor, glyph);

                return float4(final, 1);
            }

            ENDHLSL
        }
    }
}
