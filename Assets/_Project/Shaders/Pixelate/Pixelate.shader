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

            // Aggiunto blocco CBUFFER per le performance (SRP Batcher)
            CBUFFER_START(UnityPerMaterial)
                int _PixelateX;
                int _PixelateY;
            CBUFFER_END

            float4 frag(Varyings input) : SV_Target
            {
                // Configura l'instancing stereoscopico per la VR all'interno del fragment
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                if (_PixelateX <= 1 && _PixelateY <= 1)
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // Calcola le dimensioni
                float2 pixelSize = 1.0 / float2(_ScreenParams.x, _ScreenParams.y);
                float2 blockSize = pixelSize * float2(_PixelateX, _PixelateY);
                
                // Trova l'origine del "macropixel"
                float2 block = floor(input.texcoord / blockSize) * blockSize;

                // CAMPIONAMENTO SINGOLO (Ottimizzato per VR)
                // Invece di 9 campionamenti, ne facciamo 1 solo esattamente al centro del blocco.
                // L'effetto visivo "pixel art" rimane intatto, ma il costo sulle performance crolla del 90%.
                float2 centerUV = block + (blockSize * 0.5);
                
                // Nota: In VR con Single Pass, usiamo _X per assicurarci di leggere l'occhio giusto se la texture è un array
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, centerUV);
            }
            ENDHLSL
        }
    }
}