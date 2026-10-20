using System.IO;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class StudioToolsGuide
    {
        public const string GuidePath = "Docs/MrX Studio Tools.md";

        [MenuItem("Tools/MrX Studio/Tools Guide", priority = 1999)]
        public static void Open()
        {
            string path = Path.GetFullPath(GuidePath);
            if (!File.Exists(path))
            {
                Debug.LogError("MrX Studio: tools guide missing at " + path);
                return;
            }
            EditorUtility.OpenWithDefaultApp(path);
        }
    }
}
