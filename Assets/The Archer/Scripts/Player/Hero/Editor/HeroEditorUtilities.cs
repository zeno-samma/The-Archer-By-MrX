using MrX.Armory;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace MrX
{
    public static class HeroEditorUtilities
    {
        public static void CloneHero(HeroData heroToClone, string heroName)
        {
            var heroClone = ScriptableObject.CreateInstance<HeroData>();
            EditorUtility.CopySerialized(heroToClone, heroClone);

            var oldPrefab = heroToClone.Prefab;
            var oldPrefabPath = AssetDatabase.GetAssetPath(oldPrefab);
            var prefabFolderPath = System.IO.Path.GetDirectoryName(oldPrefabPath);
            var newClonePrefabPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(prefabFolderPath, heroName + ".prefab"));
            AssetDatabase.CopyAsset(oldPrefabPath, newClonePrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var newPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(newClonePrefabPath);

            using (var heroSO = new SerializedObject(heroClone))
            {
                heroSO.FindProperty("guid").stringValue = System.Guid.NewGuid().ToString();
                heroSO.FindProperty("heroName").stringValue = heroName;
                heroSO.FindProperty("prefab").objectReferenceValue = newPrefab;

                heroSO.ApplyModifiedProperties();
            }

            var path = AssetDatabase.GetAssetPath(heroToClone);
            var folderPath = System.IO.Path.GetDirectoryName(path);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(System.IO.Path.Combine(folderPath, heroName + ".asset"));
            AssetDatabase.CreateAsset(heroClone, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var databasesGUID = AssetDatabase.FindAssets($"t:ArmoryDatabase");
            foreach (var databaseGUID in databasesGUID)
            {
                var databasePath = AssetDatabase.GUIDToAssetPath(databaseGUID);
                var database = AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(databasePath);

                if(database.Heroes.Contains(heroToClone))
                {
                    Undo.RecordObject(database, "Add Cloned Hero");
                    database.AddClonedHero(heroClone, heroToClone);
                    EditorUtility.SetDirty(database);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = heroClone;
            EditorGUIUtility.PingObject(heroClone);
        }
    }
}