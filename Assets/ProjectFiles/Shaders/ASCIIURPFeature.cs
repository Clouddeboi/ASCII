using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ASCIIURPFeature : ScriptableRendererFeature
{
    class ASCIIPass : ScriptableRenderPass
    {
        public Material material;
        RTHandle tempTexture;
        RTHandle source;

        public void Setup(RTHandle src)
        {
            source = src;
        }

        public override void OnCameraSetup(
            CommandBuffer cmd,
            ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(
                ref tempTexture,
                desc,
                name: "_ASCII_Temp");
        }

        public override void Execute(
            ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            if (!material) return;

            CommandBuffer cmd = CommandBufferPool.Get("ASCII Pass");

            Blit(cmd, source, tempTexture, material, 0);
            Blit(cmd, tempTexture, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempTexture?.Release();
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
        pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }
}
