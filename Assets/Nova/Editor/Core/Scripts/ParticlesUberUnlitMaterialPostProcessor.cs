// --------------------------------------------------------------
// Copyright 2022 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Nova.Editor.Core.Scripts
{
    /// <summary>
    ///     Perform processing after properties of the material with the PerticlesUberUnlit shader have been changed.
    /// </summary>
    public static class ParticlesUberUnlitMaterialPostProcessor
    {
        private static readonly int CullId = Shader.PropertyToID(MaterialPropertyNames.Cull);
        private static readonly int VertexAlphaModeId = Shader.PropertyToID(MaterialPropertyNames.VertexAlphaMode);
        private static readonly int BaseMapModeId = Shader.PropertyToID(MaterialPropertyNames.BaseMapMode);

        private static readonly int BaseMapMirrorSamplingId =
            Shader.PropertyToID(MaterialPropertyNames.BaseMapMirrorSampling);

        private static readonly int BaseMapId = Shader.PropertyToID(MaterialPropertyNames.BaseMap);
        private static readonly int BaseMap2DArrayId = Shader.PropertyToID(MaterialPropertyNames.BaseMap2DArray);
        private static readonly int BaseMap3DId = Shader.PropertyToID(MaterialPropertyNames.BaseMap3D);
        private static readonly int BaseMapRotationId = Shader.PropertyToID(MaterialPropertyNames.BaseMapRotation);

        private static readonly int BaseMapRotationCoordId =
            Shader.PropertyToID(MaterialPropertyNames.BaseMapRotationCoord);

        private static readonly int TintAreaModeId = Shader.PropertyToID(MaterialPropertyNames.TintAreaMode);
        private static readonly int TintMapModeId = Shader.PropertyToID(MaterialPropertyNames.TintColorMode);
        private static readonly int FlowMapTargetId = Shader.PropertyToID(MaterialPropertyNames.FlowMapTarget);

        private static readonly int FlowIntensityCoordId =
            Shader.PropertyToID(MaterialPropertyNames.FlowIntensityCoord);

        private static readonly int ColorCorrectionModeId =
            Shader.PropertyToID(MaterialPropertyNames.ColorCorrectionMode);

        private static readonly int AlphaTransitionMapId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionMap);

        private static readonly int AlphaTransitionMap2DArrayId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionMap2DArray);

        private static readonly int AlphaTransitionMap3DId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionMap3D);

        private static readonly int AlphaTransitionProgressCoordId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionProgressCoord);

        private static readonly int AlphaTransitionModeId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionMode);

        private static readonly int AlphaTransitionMapModeId =
            Shader.PropertyToID(MaterialPropertyNames.AlphaTransitionMapMode);

        private static readonly int EmissionAreaTypeId = Shader.PropertyToID(MaterialPropertyNames.EmissionAreaType);
        private static readonly int EmissionColorTypeId = Shader.PropertyToID(MaterialPropertyNames.EmissionColorType);
        private static readonly int EmissionMapModeId = Shader.PropertyToID(MaterialPropertyNames.EmissionMapMode);

        private static readonly int RimTransparencyEnabledId =
            Shader.PropertyToID(MaterialPropertyNames.RimTransparencyEnabled);

        private static readonly int LuminanceTransparencyEnabledId =
            Shader.PropertyToID(MaterialPropertyNames.LuminanceTransparencyEnabled);

        private static readonly int SoftParticlesEnabledId =
            Shader.PropertyToID(MaterialPropertyNames.SoftParticlesEnabled);

        private static readonly int DepthFadeEnabledId = Shader.PropertyToID(MaterialPropertyNames.DepthFadeEnabled);
        private static readonly int RenderTypeId = Shader.PropertyToID(MaterialPropertyNames.RenderType);
        private static readonly int QueueOffsetId = Shader.PropertyToID(MaterialPropertyNames.QueueOffset);
        private static readonly int BlendSrcId = Shader.PropertyToID(MaterialPropertyNames.BlendSrc);
        private static readonly int BlendDstId = Shader.PropertyToID(MaterialPropertyNames.BlendDst);
        private static readonly int ZWriteId = Shader.PropertyToID(MaterialPropertyNames.ZWrite);

        private static readonly int TransparentBlendModeId =
            Shader.PropertyToID(MaterialPropertyNames.TransparentBlendMode);

        public static void SetupMaterialKeywords(Material material)
        {
            SetupDrawSettingsMaterialKeywords(material);
            SetupBaseColorMaterialKeywords(material);
            SetupFlowMapMaterialKeywords(material);
            SetupAlphaTransitionMaterialKeywords(material);
            SetupEmissionMaterialKeywords(material);
            SetupTransparencyMaterialKeywords(material);
        }

        private static void SetupDrawSettingsMaterialKeywords(Material material)
        {
            var renderFace = (RenderFace)material.GetFloat(CullId);
            material.doubleSidedGI = renderFace != RenderFace.Front;

            var vertexAlphaMode = (VertexAlphaMode)material.GetFloat(VertexAlphaModeId);
            var vertexAlphaAsTransitionProgressEnabled = vertexAlphaMode == VertexAlphaMode.TransitionProgress;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.VertexAlphaAsTransitionProgress,
                vertexAlphaAsTransitionProgressEnabled);
        }

        private static void SetupBaseColorMaterialKeywords(Material material)
        {
            var baseMapMode = (BaseMapMode)material.GetFloat(BaseMapModeId);
            foreach (BaseMapMode value in Enum.GetValues(typeof(BaseMapMode)))
            {
                var isOn = baseMapMode == value;
                var keyword = value.GetShaderKeyword();
                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }

            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStatePointMirror, false);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStateLinearMirror, false);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStateTrilinearMirror, false);
            if (material.GetFloat(BaseMapMirrorSamplingId) >= 0.5f)
            {
                Texture baseMap;
                switch (baseMapMode)
                {
                    case BaseMapMode.SingleTexture:
                        baseMap = material.GetTexture(BaseMapId);
                        break;
                    case BaseMapMode.FlipBook:
                        baseMap = material.GetTexture(BaseMap2DArrayId);
                        break;
                    case BaseMapMode.FlipBookBlending:
                        baseMap = material.GetTexture(BaseMap3DId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (baseMap != null)
                    switch (baseMap.filterMode)
                    {
                        case FilterMode.Point:
                            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStatePointMirror,
                                true);
                            break;
                        case FilterMode.Bilinear:
                            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStateLinearMirror,
                                true);
                            break;
                        case FilterMode.Trilinear:
                            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseSamplerStateTrilinearMirror,
                                true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }

            var baseMapRotationEnabled = material.GetFloat(BaseMapRotationId) != 0
                                         || (CustomCoord)material.GetFloat(BaseMapRotationCoordId) !=
                                         CustomCoord.Unused;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.BaseMapRotationEnabled, baseMapRotationEnabled);

            var tintAreaMode = (TintAreaMode)material.GetFloat(TintAreaModeId);
            foreach (TintAreaMode value in Enum.GetValues(typeof(TintAreaMode)))
            {
                var isOn = tintAreaMode == value;
                var keyword = value.GetShaderKeyword();
                if (string.IsNullOrEmpty(keyword)) continue;

                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }

            var tintColorMode = (TintColorMode)material.GetFloat(TintMapModeId);
            foreach (TintColorMode value in Enum.GetValues(typeof(TintColorMode)))
            {
                var isOn = tintColorMode == value;
                var keyword = value.GetShaderKeyword();
                if (string.IsNullOrEmpty(keyword)) continue;

                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }

            var colorCorrectionMode =
                (ColorCorrectionMode)material.GetFloat(ColorCorrectionModeId);
            foreach (ColorCorrectionMode value in Enum.GetValues(typeof(ColorCorrectionMode)))
            {
                var isOn = colorCorrectionMode == value;
                var keyword = value.GetShaderKeyword();
                if (string.IsNullOrEmpty(keyword)) continue;

                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }
        }

        private static void SetupFlowMapMaterialKeywords(Material material)
        {
            var flowMapTarget = (FlowMapTarget)material.GetFloat(FlowMapTargetId);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.FlowMapTargetBase,
                (flowMapTarget & FlowMapTarget.BaseMap) != 0);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.FlowMapTargetTint,
                (flowMapTarget & FlowMapTarget.TintMap) != 0);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.FlowMapTargetAlphaTransition,
                (flowMapTarget & FlowMapTarget.AlphaTransitionMap) != 0);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.FlowMapTargetEmission,
                (flowMapTarget & FlowMapTarget.EmissionMap) != 0);
        }

        private static void SetupAlphaTransitionMaterialKeywords(Material material)
        {
            var alphaTransitionEnabled = material.GetTexture(AlphaTransitionMapId) != null
                                         || material.GetTexture(AlphaTransitionMap2DArrayId) != null
                                         || material.GetTexture(AlphaTransitionMap3DId) != null;
            var alphaTransitionMode = (AlphaTransitionMode)material.GetFloat(AlphaTransitionModeId);
            var fadeTransitionEnabled = alphaTransitionEnabled && alphaTransitionMode == AlphaTransitionMode.Fade;
            var dissolveTransitionEnabled =
                alphaTransitionEnabled && alphaTransitionMode == AlphaTransitionMode.Dissolve;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.FadeTransitionEnabled, fadeTransitionEnabled);
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.DissolveTransitionEnabled,
                dissolveTransitionEnabled);

            var alphaTransitionMapMode = (AlphaTransitionMapMode)material.GetFloat(AlphaTransitionMapModeId);
            foreach (AlphaTransitionMapMode value in Enum.GetValues(typeof(AlphaTransitionMapMode)))
            {
                var isOn = alphaTransitionMapMode == value;
                var keyword = value.GetShaderKeyword();
                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }
        }

        private static void SetupEmissionMaterialKeywords(Material material)
        {
            var emissionAreaType = (EmissionAreaType)material.GetFloat(EmissionAreaTypeId);
            foreach (EmissionAreaType value in Enum.GetValues(typeof(EmissionAreaType)))
            {
                var isOn = emissionAreaType == value;
                var keyword = value.GetShaderKeyword();
                if (string.IsNullOrEmpty(keyword)) continue;

                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }

            var emissionColorType = (EmissionColorType)material.GetFloat(EmissionColorTypeId);
            foreach (EmissionColorType value in Enum.GetValues(typeof(EmissionColorType)))
            {
                var isOn = emissionColorType == value;
                var keyword = value.GetShaderKeyword();
                if (string.IsNullOrEmpty(keyword)) continue;

                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }

            var emissionMapMode = (EmissionMapMode)material.GetFloat(EmissionMapModeId);
            foreach (EmissionMapMode value in Enum.GetValues(typeof(EmissionMapMode)))
            {
                var isOn = emissionMapMode == value;
                var keyword = value.GetShaderKeyword();
                MaterialEditorUtility.SetKeyword(material, keyword, isOn);
            }
        }

        private static void SetupTransparencyMaterialKeywords(Material material)
        {
            var rimTransparencyEnabled = material.GetFloat(RimTransparencyEnabledId) > 0.5f;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.TransparencyByRim, rimTransparencyEnabled);
            var luminanceTransparencyEnabled =
                material.GetFloat(LuminanceTransparencyEnabledId) > 0.5f;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.TransparencyByLuminance,
                luminanceTransparencyEnabled);

            var softParticlesEnabled = material.GetFloat(SoftParticlesEnabledId) > 0.5f;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.SoftParticlesEnabled, softParticlesEnabled);

            var depthFadeEnabled = material.GetFloat(DepthFadeEnabledId) > 0.5f;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.DepthFadeEnabled, depthFadeEnabled);
        }

        public static void SetupMaterialBlendMode(Material material)
        {
            var renderType = (RenderType)material.GetFloat(RenderTypeId);
            var alphaClip = renderType == RenderType.Cutout;
            MaterialEditorUtility.SetKeyword(material, ShaderKeywords.AlphaTestEnabled, alphaClip);

            if (renderType == RenderType.Opaque)
            {
                material.renderQueue = (int)RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
                material.renderQueue += (int)material.GetFloat(QueueOffsetId);
                material.SetInt(BlendSrcId, (int)BlendMode.One);
                material.SetInt(BlendDstId, (int)BlendMode.Zero);
                material.SetInt(ZWriteId, 1);
                material.DisableKeyword(ShaderKeywords.AlphaModulateEnabled);
            }
            else if (renderType == RenderType.Cutout)
            {
                material.renderQueue = (int)RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.renderQueue += (int)material.GetFloat(QueueOffsetId);
                material.SetInt(BlendSrcId, (int)BlendMode.One);
                material.SetInt(BlendDstId, (int)BlendMode.Zero);
                material.SetInt(ZWriteId, 1);
                material.DisableKeyword(ShaderKeywords.AlphaModulateEnabled);
            }
            else if (renderType == RenderType.Transparent)
            {
                var blendMode = (TransparentBlendMode)material.GetFloat(TransparentBlendModeId);

                switch (blendMode)
                {
                    case TransparentBlendMode.Alpha:
                        material.SetInt(BlendSrcId, (int)BlendMode.SrcAlpha);
                        material.SetInt(BlendDstId, (int)BlendMode.OneMinusSrcAlpha);
                        material.DisableKeyword(ShaderKeywords.AlphaModulateEnabled);
                        break;
                    case TransparentBlendMode.Additive:
                        material.SetInt(BlendSrcId, (int)BlendMode.SrcAlpha);
                        material.SetInt(BlendDstId, (int)BlendMode.One);
                        material.DisableKeyword(ShaderKeywords.AlphaModulateEnabled);
                        break;
                    case TransparentBlendMode.Multiply:
                        material.SetInt(BlendSrcId, (int)BlendMode.DstColor);
                        material.SetInt(BlendDstId, (int)BlendMode.Zero);
                        material.EnableKeyword(ShaderKeywords.AlphaModulateEnabled);
                        break;
                }

                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt(ZWriteId, 0);
                material.renderQueue = (int)RenderQueue.Transparent;
                material.renderQueue += (int)material.GetFloat(QueueOffsetId);
            }
        }
    }
}