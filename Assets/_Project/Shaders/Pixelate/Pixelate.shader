Shader "Custom/Pixelate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelateX("Pixelate X", Int) = 5
        _PixelateY("Pixelate Y", Int) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            int _PixelateX;
            int _PixelateY;

            float4 frag(Varyings input) : SV_Target
            {
                if (_PixelateX <= 1 && _PixelateY <= 1)
                    return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                float2 pixelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float2 blockSize = pixelSize * float2(_PixelateX, _PixelateY);
                float2 block = float2(
                    floor(input.texcoord.x / blockSize.x) * blockSize.x,
                    floor(input.texcoord.y / blockSize.y) * blockSize.y
                );

                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + blockSize * 0.5);
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.25, blockSize.y * 0.25));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.5,  blockSize.y * 0.25));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.75, blockSize.y * 0.25));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.25, blockSize.y * 0.5));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.75, blockSize.y * 0.5));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.25, blockSize.y * 0.75));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.5,  blockSize.y * 0.75));
                col += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, block + float2(blockSize.x * 0.75, blockSize.y * 0.75));
                return col / 9.0;
            }
            ENDHLSL
        }
    }
}