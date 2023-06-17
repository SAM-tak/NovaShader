// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Nova.Runtime.Core.Scripts
{
    [Serializable]
    public sealed class ScreenSpaceDistortion : ScriptableRendererFeature
    {
        private const string DistortionLightMode = "DistortedUvBuffer";

        [SerializeField] private bool _applyToSceneView = true;
        [SerializeField] [HideInInspector] private Shader _applyDistortionShader;
        private ApplyDistortionPass _applyDistortionPass;

        private DistortedUvBufferPass _distortedUvBufferPass;

		private RTHandle _distortedUvBufferHandle;

		public override void Create()
        {
            _applyDistortionShader = Shader.Find("Hidden/Nova/Particles/ApplyDistortion");
            if (_applyDistortionShader == null) return;

            _distortedUvBufferPass = new DistortedUvBufferPass(DistortionLightMode);
            _applyDistortionPass = new ApplyDistortionPass(_applyToSceneView, _applyDistortionShader);

			_distortedUvBufferHandle = RTHandles.Alloc("distortedUvBuffer");
		}

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_applyDistortionShader == null || renderingData.cameraData.cameraType == CameraType.Reflection) return;
            var cameraTargetDesciptor = renderingData.cameraData.cameraTargetDescriptor;

            var distortedUvBufferFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf)
                ? RenderTextureFormat.RGHalf
                : RenderTextureFormat.DefaultHDR;
            var distortedUvBuffer = RenderTexture.GetTemporary(cameraTargetDesciptor.width,
                cameraTargetDesciptor.height, 0, distortedUvBufferFormat, RenderTextureReadWrite.Default,
                cameraTargetDesciptor.msaaSamples);
            var distortedUvBufferIdentifier = new RenderTargetIdentifier(distortedUvBuffer);

            _distortedUvBufferPass.Setup(_distortedUvBufferHandle, () => renderer.cameraDepthTargetHandle);
            _applyDistortionPass.Setup(renderer, _distortedUvBufferHandle);
            renderer.EnqueuePass(_distortedUvBufferPass);
            renderer.EnqueuePass(_applyDistortionPass);
            RenderTexture.ReleaseTemporary(distortedUvBuffer);
        }
    }
}
