// Unity built-in source. Copyright (c) Unity Technologies.
// Modified by October Studio.
// Licensed under the Unity Companion License (see https://unity3d.com/legal/licenses/unity-companion-license)

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

namespace OctoberStudio
{
    public class StandardParticlesScrollingShaderGUIURP : BaseShaderGUI
    {
        private BakedLitGUI.BakedLitProperties shadingModelProperties;
        private ParticleGUI.ParticleProperties particleProps;

        MaterialProperty scrollingVelocity = null;
        MaterialProperty rimColor = null;
        MaterialProperty rimStrength = null;
        MaterialProperty rimGradient = null;
        MaterialProperty rimDirectional = null;

        // List of renderers using this material in the scene, used for validating vertex streams
        List<ParticleSystemRenderer> m_RenderersUsingThisMaterial = new List<ParticleSystemRenderer>();

        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            shadingModelProperties = new BakedLitGUI.BakedLitProperties(properties);
            particleProps = new ParticleGUI.ParticleProperties(properties);

            scrollingVelocity = FindProperty("_ScrollingVelocity", properties);
            rimColor = FindProperty("_RimColor", properties);
            rimStrength = FindProperty("_RimStrength", properties);
            rimGradient = FindProperty("_RimGradient", properties);
            rimDirectional = FindProperty("_RimDirectional", properties);
        }

        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material, null, ParticleGUI.SetMaterialKeywords);
        }

        public override void DrawSurfaceOptions(Material material)
        {
            base.DrawSurfaceOptions(material);
            DoPopup(ParticleGUI.Styles.colorMode, particleProps.colorMode, Enum.GetNames(typeof(ParticleGUI.ColorMode)));
        }

        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            BakedLitGUI.Inputs(shadingModelProperties, materialEditor);
            DrawEmissionProperties(material, true);
        }

        public override void DrawAdvancedOptions(Material material)
        {
            materialEditor.ShaderProperty(particleProps.flipbookMode, ParticleGUI.Styles.flipbookMode);
            ParticleGUI.FadingOptions(material, materialEditor, particleProps);
            ParticleGUI.DoVertexStreamsArea(material, m_RenderersUsingThisMaterial);

            DrawQueueOffsetField();
        }

        public override void OnOpenGUI(Material material, MaterialEditor materialEditor)
        {
            CacheRenderersUsingThisMaterial(material);
            base.OnOpenGUI(material, materialEditor);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditorIn, properties);

            materialEditorIn.VectorProperty(scrollingVelocity, scrollingVelocity.displayName);

            materialEditorIn.ColorProperty(rimColor, rimColor.displayName);
            materialEditorIn.RangeProperty(rimStrength, rimStrength.displayName);
            materialEditorIn.RangeProperty(rimGradient, rimGradient.displayName);
            materialEditorIn.RangeProperty(rimDirectional, rimDirectional.displayName);
        }

        void CacheRenderersUsingThisMaterial(Material material)
        {
            m_RenderersUsingThisMaterial.Clear();

#if UNITY_6000_4_OR_NEWER
            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsByType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
#elif UNITY_6000_0_OR_NEWER
            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsByType(typeof(ParticleSystemRenderer), FindObjectsSortMode.None) as ParticleSystemRenderer[];
#else
            ParticleSystemRenderer[] renderers = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystemRenderer)) as ParticleSystemRenderer[];
#endif
            foreach (ParticleSystemRenderer renderer in renderers)
            {
                if (renderer.sharedMaterial == material)
                    m_RenderersUsingThisMaterial.Add(renderer);
            }
        }
    }
}