using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PixelateRenderPass : ScriptableRenderPass
{
    PixelateRendererFeature.Settings _settings;
    Material _material;

    public PixelateRenderPass(PixelateRendererFeature.Settings settings)
    {
        _settings = settings;
        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        Debug.Log("RecordRenderGraph chiamato");

        if (_settings.shader == null)
        {
            Debug.LogError("Shader è NULL!");
            return;
        }

        if (_material == null)
            _material = new Material(_settings.shader);

        Debug.Log($"Material creato: {_material != null}, shader: {_settings.shader.name}");

        int sizeX = _settings.pixelSizeX;
        int sizeY = _settings.lockXY ? sizeX : _settings.pixelSizeY;
        _material.SetInt("_PixelateX", sizeX);
        _material.SetInt("_PixelateY", sizeY);

        Debug.Log($"PixelateX: {sizeX}, PixelateY: {sizeY}");

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        Debug.Log($"activeColorTexture valida: {resourceData.activeColorTexture.IsValid()}");

        var desc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
        desc.name = "PixelateTemp";
        desc.clearBuffer = false;
        TextureHandle tempTexture = renderGraph.CreateTexture(desc);
        Debug.Log($"tempTexture valida: {tempTexture.IsValid()}");

        RenderGraphUtils.BlitMaterialParameters para = new(
            resourceData.activeColorTexture, tempTexture, _material, 0);
        renderGraph.AddBlitPass(para, "Pixelate Blit");
        Debug.Log("Primo blit aggiunto");

        renderGraph.AddCopyPass(tempTexture, resourceData.activeColorTexture, "Pixelate Copy Back");
        Debug.Log("CopyPass aggiunto");
    }
}