using System;
using System.Linq;
using MrX.Armory;
using MrX.Progression;
using MrX.UI;
using MrX.Weapon;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MrX.EditorTools
{
    [InitializeOnLoad]
    internal static class MagePlaytest
    {
        private const string MenuPath = "Tools/MrX Studio/Playtests/Mage";
        private const string MainMenuPath = "Assets/The Archer/Scenes/Main Menu.unity";
        private const string DatabasePath = "Assets/Mage Playtest/Editor/Content/Mage Trial Armory.asset";
        private const string ArmoryPath = "Assets/The Archer/Scriptables/Armory/";
        private const string MagePath = ArmoryPath + "Heroes/Mage.asset";
        private const string WandPath = ArmoryPath + "Items/Apprentice Wand.asset";
        private const string CrossbowPath = ArmoryPath + "Items/Crossbow.asset";
        private const string ActiveKey = "MrX.MagePlaytest.Active";
        private const string SeededKey = "MrX.MagePlaytest.Seeded";
        private const string PreviousSceneKey = "MrX.MagePlaytest.PreviousScene";

        static MagePlaytest()
        {
            SceneManager.sceneLoaded += UseTrialArmory;
            EditorApplication.update += PrepareTrial;
            EditorApplication.playModeStateChanged += RestoreEditor;
        }

        [MenuItem(MenuPath, true)]
        private static bool CanStart() => !EditorApplication.isPlayingOrWillChangePlaymode &&
            SceneManager.GetSceneByPath(MainMenuPath).isLoaded;

        [MenuItem(MenuPath)]
        internal static void Start()
        {
            if (!CanStart() || SessionState.GetBool(ActiveKey, false)) return;
            var startup = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);
            if (!startup || !AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(DatabasePath))
                throw new InvalidOperationException("Mage playtest assets are missing.");

            Debug.Log(VerificationSession.Begin());
            SessionState.SetString(PreviousSceneKey, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(ActiveKey, true);
            SessionState.EraseBool(SeededKey);
            EditorSceneManager.playModeStartScene = startup;
            EditorApplication.EnterPlaymode();
        }

        private static void UseTrialArmory(Scene scene, LoadSceneMode mode)
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(ActiveKey, false) || scene.path != MainMenuPath ||
                SessionState.GetBool(SeededKey, false)) return;

            var trial = AssetDatabase.LoadAssetAtPath<ArmoryDatabase>(DatabasePath);
            var armory = GameController.ArmoryManager;
            if (!armory || !trial)
            {
                Debug.LogError("Could not bind the isolated Mage Armory.");
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                return;
            }

            // Awake registers the manager. Its Start reads this database to initialize the isolated save.
            using var serialized = new SerializedObject(armory);
            serialized.FindProperty("database").objectReferenceValue = trial;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PrepareTrial()
        {
            if (!EditorApplication.isPlaying || !SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(SeededKey, false)) return;
            var menu = GameController.MainMenuScreenBehavior;
            var armory = GameController.ArmoryManager;
            if (!menu || menu.CurrentPage == null || !menu.IsDockEnabled || !armory || armory.EquippedHero == null || Time.frameCount < 3) return;

            SessionState.SetBool(SeededKey, true);
            try
            {
                var mage = AssetDatabase.LoadAssetAtPath<HeroData>(MagePath);
                var wand = AssetDatabase.LoadAssetAtPath<WeaponData>(WandPath);
                var crossbow = AssetDatabase.LoadAssetAtPath<WeaponData>(CrossbowPath);
                if (!mage || !wand || !crossbow || !armory.GetAllHeroDataList().Contains(mage) ||
                    armory.GetItemData(wand.Id) != wand || armory.GetItemData(crossbow.Id) != crossbow)
                    throw new InvalidOperationException("The Mage trial hero or weapons are missing from the trial Armory.");

                if (PlayerProgression.Current.Level < PlayerProgression.RequiredLevel(MainMenuPageType.Heroes))
                {
                    var grantXp = typeof(PlayerProgression).GetMethod("ChangeDeveloperXp",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                    if (grantXp == null || !(bool)grantXp.Invoke(null, new object[] { 300, false }))
                        throw new InvalidOperationException("Could not unlock the Hero menu in the trial save.");
                }

                var heroSave = armory.GetHeroSave(mage.Id);
                heroSave.TryAddShards(50);
                GameController.CurrenciesManager.GetCurrency("gold", false).Deposit(2000);
                if (!armory.IsItemUnlocked(wand)) armory.UnlockItem(wand);
                if (!armory.IsItemUnlocked(crossbow)) armory.UnlockItem(crossbow);
                armory.GetItemSaveList().Single(item => item.Id == wand.Id).IsEquipped = true;
                heroSave.IsEquipped = true;
                if (!menu.OpenPage(MainMenuPageType.Heroes))
                    throw new InvalidOperationException("Could not open the Hero menu in the trial save.");
                GameController.SaveManager.Save(false);
                Debug.Log($"Mage trial ready. Mage and {wand.ItemName} are equipped. Use Journey to enter the stage.");
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.ExitPlaymode();
            }
        }

        private static void RestoreEditor(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(ActiveKey, false)) return;
            string previous = SessionState.GetString(PreviousSceneKey, "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous)
                ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
            SessionState.EraseString(PreviousSceneKey);
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(SeededKey);
            Debug.Log(VerificationSession.End());
        }
    }
}
