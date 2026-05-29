using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Shader shader;
        [Range(1, 20)] public int pixelSizeX = 1;
        [Range(1, 20)] public int pixelSizeY = 1;
        public bool lockXY = true;
    }

    public Settings settings = new Settings();
    PixelateRenderPass _pass;

    public override void Create()
    {
        _pass = new PixelateRenderPass(settings);
        _pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.shader == null) return;
        renderer.EnqueuePass(_pass);
    }
}