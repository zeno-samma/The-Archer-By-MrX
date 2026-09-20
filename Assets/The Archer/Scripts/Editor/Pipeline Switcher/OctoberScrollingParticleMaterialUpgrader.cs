using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OctoberStudio.PipelineSwitcher 
{
    public class OctoberScrollingParticleMaterialUpgrader : MaterialUpgrader
    {
        public OctoberScrollingParticleMaterialUpgrader()
        {
            RenameFloat("_Mode", "_Surface");

            RenameShader("October/Particles/Standard Unlit Scrolling", "October/Particles/Standard Unlit Scrolling URP", UpdateUnlit);

            RenameTexture("_MainTex", "_BaseMap");
            RenameColor("_Color", "_BaseColor");
            RenameFloat("_FlipbookMode", "_FlipbookBlending");
        }

        /// <summary>
        /// Updates the unlit shader properties.
        /// </summary>
        /// <param name="material"></param>
        public static void UpdateUnlit(Material material)
        {
            UpdateSurfaceBlendModes(material);
            DisableKeywords(material);
            ApplyBlendState(material);
            FinalizeMaterial(material);
        }

        /// <summary>
        /// Updates the blending mode properties.
        /// </summary>
        /// <param name="material"></param>
        public static void UpdateSurfaceBlendModes(Material material)
        {
            switch (material.GetFloat("_Mode"))
            {
                case 0: // opaque
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Opaque);
                    break;
                case 1: // cutout > alphatest
                    material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Opaque);
                    material.SetFloat("_AlphaClip", 1);
                    break;
                case 2: // fade > alpha
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Transparent);
                    material.SetFloat("_Blend", (int)UpgradeBlendMode.Alpha);
                    break;
                case 3: // transparent > premul
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Transparent);
                    material.SetFloat("_Blend", (int)UpgradeBlendMode.Premultiply);
                    break;
                case 4: // add
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Transparent);
                    material.SetFloat("_Blend", (int)UpgradeBlendMode.Additive);
                    break;
                case 5: // sub > none
                    break;
                case 6: // mod > multiply
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.SetFloat("_Surface", (int)UpgradeSurfaceType.Transparent);
                    material.SetFloat("_Blend", (int)UpgradeBlendMode.Multiply);
                    break;
            }

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            CoreUtils.SetKeyword(material, ShaderKeywordStrings._EMISSION, material.HasProperty("_EmissionEnabled") && material.GetFloat("_EmissionEnabled") >= 0.5f);
        }

        public static void DisableKeywords(Material material)
        {
            // LOD fade is now controlled by the render pipeline, and not the individual material, so disable it.
            material.DisableKeyword("LOD_FADE_CROSSFADE");
        }

        public static void FinalizeMaterial(Material material)
        {
            var isTransparent = material.GetFloat("_Surface") == (int)UpgradeSurfaceType.Transparent;

            material.SetOverrideTag("RenderType", isTransparent ? "Transparent" : "Opaque");

            material.renderQueue = isTransparent
                ? (int)UnityEngine.Rendering.RenderQueue.Transparent
                : (int)UnityEngine.Rendering.RenderQueue.Geometry;

            material.SetShaderPassEnabled("DepthOnly", !isTransparent);
            material.SetShaderPassEnabled("ShadowCaster", !isTransparent);

            CoreUtils.SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", isTransparent);
        }

        protected static void ApplyBlendState(Material material)
        {
            var surface = (int)material.GetFloat("_Surface");
            var blend = material.HasProperty("_Blend") ? (int)material.GetFloat("_Blend") : 0;

            var isTransparent = surface == (int)UpgradeSurfaceType.Transparent;

            if (!isTransparent)
            {
                material.SetFloat("_SrcBlend", (int)BlendMode.One);
                material.SetFloat("_DstBlend", (int)BlendMode.Zero);
                material.SetFloat("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            else
            {
                material.SetFloat("_ZWrite", 0);

                switch (blend)
                {
                    case (int)UpgradeBlendMode.Alpha:
                        material.SetFloat("_SrcBlend", (int)BlendMode.SrcAlpha);
                        material.SetFloat("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;

                    case (int)UpgradeBlendMode.Premultiply:
                        material.SetFloat("_SrcBlend", (int)BlendMode.One);
                        material.SetFloat("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;

                    case (int)UpgradeBlendMode.Additive:
                        material.SetFloat("_SrcBlend", (int)BlendMode.SrcAlpha);
                        material.SetFloat("_DstBlend", (int)BlendMode.One);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;

                    case (int)UpgradeBlendMode.Multiply:
                        material.SetFloat("_SrcBlend", (int)BlendMode.DstColor);
                        material.SetFloat("_DstBlend", (int)BlendMode.Zero);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                }
            }
        }
    }
}