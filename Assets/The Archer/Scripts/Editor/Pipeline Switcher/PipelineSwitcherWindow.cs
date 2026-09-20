using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace OctoberStudio.PipelineSwitcher
{
    public class PipelineSwitcherWindow : EditorWindow
    {
        public static readonly string URP_PACKAGE = "com.unity.render-pipelines.universal";
        public static readonly string THE_ARCHER_URP_ASSETS = "Assets/The Archer/Pipeline Switcher/The Archer URP Assets.unitypackage";
        public static readonly string STARTED_INSTALLING_URP = "Pipeline Switcher: Started Installing URP";
        public static readonly string STARTED_UNPACKING_URP_SHADERS = "Pipeline Switcher: Started Unpacking URP shaders";
        public static readonly string FINISHED_SWITCHING = "Pipeline Switcher: Finished Switching";

        protected AddRequest addRequest;
        protected double startTime;

        [MenuItem("Tools/October/Pipeline Switcher")]
        public static void OpenWindow()
        {
            PipelineSwitcherWindow wnd = GetWindow<PipelineSwitcherWindow>();
            wnd.titleContent = new GUIContent("Pipeline Switcher");
            wnd.minSize = new Vector2(300, 200);
        }

        protected void OnEnable()
        {
            if (EditorPrefs.GetBool(STARTED_INSTALLING_URP, false))
            {
                EditorPrefs.SetBool(STARTED_INSTALLING_URP, false);
                SwitchToURP();
            }

            if(EditorPrefs.GetBool(STARTED_UNPACKING_URP_SHADERS, false))
            {
                EditorApplication.delayCall += FinishUnpacking;
            }
        }

        protected virtual void OnGUI()
        {
            if (EditorPrefs.GetBool(STARTED_INSTALLING_URP, false))
            {
                EditorGUILayout.LabelField("Installing URP Package...");
                return;
            }

            if (EditorPrefs.GetBool(STARTED_UNPACKING_URP_SHADERS, false))
            {
                EditorGUILayout.LabelField("Unpacking URP shaders...");
                return;
            }

            var urpInstalled = IsURPInstalled();
            var packageUnpacked = IsURPPackageUnpacked();

            if (EditorPrefs.GetBool(FINISHED_SWITCHING, false) && urpInstalled && packageUnpacked)
            {
                EditorGUILayout.LabelField("Switching to URP Finished");

                if(GUILayout.Button("Convert Materials"))
                {
                    var upgraders = new List<MaterialUpgrader> { new OctoberMaterialUpgrader(), new OctoberScrollingParticleMaterialUpgrader() };
                    MaterialUpgrader.UpgradeProjectFolder(upgraders, "Upgrading October materials...", MaterialUpgrader.UpgradeFlags.None);
                }
                return;
            }

            if (!IsURPInstalled())
            {
                EditorGUILayout.HelpBox("Press Button below to switch from Built-in renderer to URP. This tool will download and install URP Package from Package Manager, unpack URP versions of October shaders, and convert materials to work with URP", MessageType.Info);
                EditorGUILayout.HelpBox("Do not close this window until the conversion is complete!", MessageType.Warning);
                if (GUILayout.Button("Switch to URP"))
                {
                    InstallURP();
                }
            } else if (!IsURPPackageUnpacked())
            {
                EditorGUILayout.HelpBox("The URP Package is already installed from Package Manager. Press Button below unpack URP versions of October shaders, and convert materials to work with URP", MessageType.Info);
                EditorGUILayout.HelpBox("Do not close this window until the conversion is complete!", MessageType.Warning);
                if (GUILayout.Button("Unpack URP shaders"))
                {
                    SwitchToURP();
                }
            }
        }

        protected virtual void SwitchToURP()
        {
            EditorPrefs.SetBool(STARTED_UNPACKING_URP_SHADERS, true);

            Debug.Log("Started Importing package");
#if UNITY_6000_6_OR_NEWER
            UnityEditor.AssetPackage.Package.Import(THE_ARCHER_URP_ASSETS, false);
#else
            AssetDatabase.ImportPackage(THE_ARCHER_URP_ASSETS, false);
#endif
        }

        protected virtual bool IsURPInstalled()
        {
            return UnityEditor.PackageManager.PackageInfo.FindForPackageName(URP_PACKAGE) != null;
        }

        protected virtual bool IsURPPackageUnpacked()
        {
            return Type.GetType("OctoberStudio.StandardParticlesScrollingShaderGUI") != null;
        }

        protected virtual void InstallURP()
        {
            EditorPrefs.SetBool(STARTED_INSTALLING_URP, true);

            Debug.Log("Installing Universal Render Pipeline...");
            EditorUtility.DisplayProgressBar("Installing URP", "Please wait...", 0.0f);
            startTime = EditorApplication.timeSinceStartup;

            addRequest = Client.Add(URP_PACKAGE);

            EditorApplication.update += URPInstalationProgress;
        }

        protected virtual void URPInstalationProgress()
        {
            EditorUtility.DisplayProgressBar("Installing URP", "Please wait...", (float)(EditorApplication.timeSinceStartup - startTime) / 500);

            if (!addRequest.IsCompleted)
                return;

            EditorApplication.update -= URPInstalationProgress;
            EditorUtility.ClearProgressBar();

            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Installed: {addRequest.Result.displayName} {addRequest.Result.version}");
            }
            else if (addRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError("Failed to install URP: " + addRequest.Error.message);
            }
        }

        public virtual void FinishSwitching()
        {
            EditorPrefs.SetBool(STARTED_UNPACKING_URP_SHADERS, false);
            EditorPrefs.SetBool(FINISHED_SWITCHING, true);
        }

        protected virtual void FinishUnpacking()
        {
            GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/The Archer/Scriptables/URP/Universal Render Pipeline Asset.asset");
            FinishSwitching();

            string[] guids = AssetDatabase.FindAssets("t:Material");

            var materials = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(mat => mat != null)
                .ToList();

            var octoberUpgrader = new OctoberMaterialUpgrader();
            var particleUpgrader = new OctoberScrollingParticleMaterialUpgrader();
            var upgraders = new List<MaterialUpgrader> { new OctoberMaterialUpgrader(), new OctoberScrollingParticleMaterialUpgrader() };
            foreach (var material in materials)
            {
                MaterialUpgrader.Upgrade(material, upgraders, MaterialUpgrader.UpgradeFlags.None);
            }
        }
    }
}