using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoThief.Sonar
{
    /// <summary>
    /// URP Scriptable Renderer Feature that injects the sonar post-process pass.
    /// 
    /// Setup:
    /// 1. Add this renderer feature to your URP Renderer asset.
    /// 2. Assign the SonarPostProcess material (using the SonarPostProcess shader).
    /// 3. The SonarManager pushes pulse data to global shader properties each frame.
    /// </summary>
    public class SonarRendererFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("Material using the SonarPostProcess shader.")]
            public Material sonarMaterial;

            [Tooltip("When in the render pipeline this pass executes.")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public Settings settings = new Settings();
        private SonarRenderPass _renderPass;

        public override void Create()
        {
            _renderPass = new SonarRenderPass(settings);
            _renderPass.renderPassEvent = settings.renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.sonarMaterial == null)
            {
                Debug.LogWarning("[SonarRendererFeature] Sonar material is not assigned.");
                return;
            }

            renderer.EnqueuePass(_renderPass);
        }
    }

    /// <summary>
    /// Custom render pass that applies the sonar post-process effect as a full-screen blit.
    /// </summary>
    public class SonarRenderPass : ScriptableRenderPass
    {
        private readonly SonarRendererFeature.Settings _settings;
        private RenderTargetIdentifier _source;
        private RenderTargetHandle _tempTexture;

        public SonarRenderPass(SonarRendererFeature.Settings settings)
        {
            _settings = settings;
            _tempTexture.Init("_SonarTempTexture");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_settings.sonarMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("SonarPostProcess");

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(_tempTexture.id, desc);
            cmd.Blit(_source, _tempTexture.Identifier(), _settings.sonarMaterial);
            cmd.Blit(_tempTexture.Identifier(), _source);
            cmd.ReleaseTemporaryRT(_tempTexture.id);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
}
