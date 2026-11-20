using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class ArcBornUpgradeAdMobMetadata
{
    private static readonly string[] AdMobPluginAssets =
    {
        "Assets/GoogleMobileAds/GoogleMobileAds.Ump.iOS.dll",
        "Assets/GoogleMobileAds/GoogleMobileAds.iOS.dll",
        "Assets/GoogleMobileAds/GoogleMobileAds.Ump.Unity.dll",
        "Assets/GoogleMobileAds/GoogleMobileAds.Unity.dll",
        "Assets/GoogleMobileAds/GoogleMobileAds.Android.dll",
        "Assets/GoogleMobileAds/GoogleMobileAds.Ump.Android.dll",
        "Assets/Plugins/iOS/GADUAdNetworkExtras.h",
        "Assets/Plugins/Android/googlemobileads-unity.aar",
        "Assets/Plugins/iOS/NativeTemplates/GADTSmallTemplateView.xib",
        "Assets/Plugins/iOS/NativeTemplates/GADTMediumTemplateView.xib",
    };

    [MenuItem("Tools/ArcBorn/Upgrade AdMob Metadata")]
    private static void UpgradeMetadata()
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string[] existingAssets = AdMobPluginAssets
            .Where(path => File.Exists(Path.Combine(projectRoot, path)) &&
                           File.Exists(Path.Combine(projectRoot, path + ".meta")))
            .ToArray();

        if (existingAssets.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Upgrade AdMob Metadata",
                "No AdMob plugin files from the warning log were found in this project.",
                "OK");
            return;
        }

        AssetDatabase.ForceReserializeAssets(
            existingAssets,
            ForceReserializeAssetsOptions.ReserializeMetadata);
    }
}
