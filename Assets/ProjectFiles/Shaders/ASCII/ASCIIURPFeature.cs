using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class ASCIIURPFeature : ScriptableRendererFeature
{
    class ASCIIPass : ScriptableRenderPass
    {
        public Material material;

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            if (!material) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraColor = resourceData.activeColorTexture;

            var desc = renderGraph.GetTextureDesc(cameraColor);
            desc.name = "_ASCII_Temp";
            desc.clearBuffer = false;
            desc.depthBufferBits = 0;
            var tempTexture = renderGraph.CreateTexture(desc);

            RenderGraphUtils.BlitMaterialParameters blitToTemp = new(cameraColor, tempTexture, material, 0);
            renderGraph.AddBlitPass(blitToTemp, "ASCII Pass - Material Blit");

            RenderGraphUtils.BlitMaterialParameters blitToColor = new(tempTexture, cameraColor, null, 0);
            renderGraph.AddBlitPass(blitToColor, "ASCII Pass - Copy Back");
        }
    }

    public Material material;
    ASCIIPass pass;

    public override void Create()
    {
        pass = new ASCIIPass();
        pass.renderPassEvent =
            RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (!material) return;

        pass.material = material;
        renderer.EnqueuePass(pass);
    }
}
