using MrX.Quests;
using MrX.UI;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class DailyQuestsBuilder
    {
        private const string Folder = "Assets/The Archer/Resources";
        private const string PrefabPath = MenuPrefabAuthoring.Folder + "/Daily Quests Page.prefab";

        [MenuItem("Tools/MrX Studio/Daily Quests/Build Editable Page Prefab")]
        public static void BuildPrefab()
        {
            var database = AssetDatabase.LoadAssetAtPath<DailyQuestsDatabase>(Folder + "/Daily Quests Database.asset");
            if (database == null || !database.IsValid(out _))
            { Debug.LogError("Daily quests: configure a valid database before building the page."); return; }
            var page = DailyQuestsPageBehavior.Create(null, database);
            try
            {
                PrefabUtility.SaveAsPrefabAsset(page.gameObject, PrefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Daily quests: editable prefab saved at " + PrefabPath);
            }
            finally { Object.DestroyImmediate(page.gameObject); }
        }

        [MenuItem("Tools/MrX Studio/Daily Quests/Select Database")]
        public static void SelectDatabase()
        { Selection.activeObject = AssetDatabase.LoadAssetAtPath<DailyQuestsDatabase>(Folder + "/Daily Quests Database.asset"); }
    }
}
