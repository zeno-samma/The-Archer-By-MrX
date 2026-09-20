using OctoberStudio.Armory;
using OctoberStudio.Upgrades;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Currency
{
    [InitializeOnLoad]
    public static class CurrencyPriceFixer
    {
        private static readonly string FIXED_KEY_1_2_0 = "CurrencyPriceFixer Fixed v1.2.0";

        static CurrencyPriceFixer()
        {
            EditorApplication.delayCall += () => RunCheck(true);
        }

        [MenuItem("Tools/October/Migration/Migrate from Cost to Price", true)]
        private static bool RunCheckMenuValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Tools/October/Migration/Migrate from Cost to Price")]
        private static void RunCheckMenu()
        {
            RunCheck(false);
        }

        private static void RunCheck(bool checkPrefKey)
        {
            if (checkPrefKey && EditorPrefs.GetBool(FIXED_KEY_1_2_0, false)) return;

            var defaultCurrencyId = "gold";

            var database = CurrencyCache.Database;
            if(database != null)
            {
                defaultCurrencyId = database.GetCurrency(0).ID;
            }

            var changed = false;

            try
            {
                if (TryFixAllHeroData(defaultCurrencyId))
                {
                    changed = true;
                }
            }catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[Currency Price Fixer] Could not migrate from cost to price. Most likely the Hero Data structure was modified");
            }

            try
            {
                if (TryFixAllItemData(defaultCurrencyId))
                {
                    changed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[Currency Price Fixer] Could not migrate from cost to price. Most likely the Item Data structure was modified");
            }

            try
            {
                if (TryFixAllUpgradeData(defaultCurrencyId))
                {
                    changed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[Currency Price Fixer] Could not migrate from cost to price. Most likely the Upgrade Data structure was modified");
            }

            try
            {
                if (TryFixAllStageData(defaultCurrencyId))
                {
                    changed = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("[Currency Price Fixer] Could not migrate from cost to price. Most likely the Stage Data structure was modified");
            }

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[Currency Price Fixer] Migrated to Price");
            }

            EditorPrefs.SetBool(FIXED_KEY_1_2_0, true);
        }

        #region HeroData

        private static bool TryFixAllHeroData(string defaultCurrencyId)
        {
            var guids = AssetDatabase.FindAssets("t:HeroData");

            var changed = false;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                HeroData hero = AssetDatabase.LoadAssetAtPath<HeroData>(path);

                if (hero == null || hero.DataVersion == 1) continue;

                if (TryFixHeroData(hero, defaultCurrencyId))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TryFixHeroData(HeroData heroData, string defaultCurrencyId)
        {
            var serializedObject = new SerializedObject(heroData);

            var heroLevelsProperty = serializedObject.FindProperty("heroLevels");

            var changed = false;

            for (int i = 0; i < heroLevelsProperty.arraySize; i++)
            {
                var levelProperty = heroLevelsProperty.GetArrayElementAtIndex(i);
                var priceProperty = levelProperty.FindPropertyRelative("price");

                var currencyId = priceProperty.FindPropertyRelative("currencyId");

                if (string.IsNullOrEmpty(currencyId.stringValue))
                {
                    currencyId.stringValue = defaultCurrencyId;

                    var costProperty = levelProperty.FindPropertyRelative("cost");
                    var amountProperty = priceProperty.FindPropertyRelative("amount");

                    amountProperty.intValue = costProperty.intValue;

                    changed = true;
                }
            }

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if(dataVersionProperty.intValue == 0)
            {
                dataVersionProperty.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(heroData);
            }

            return changed;
        }

        #endregion

        #region ItemData

        private static bool TryFixAllItemData(string defaultCurrencyId)
        {
            var guids = AssetDatabase.FindAssets("t:ItemData");

            var changed = false;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

                if (item == null || item.DataVersion == 1) continue;

                if (TryFixItemData(item, defaultCurrencyId))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TryFixItemData(ItemData itemData, string defaultCurrencyId)
        {
            var serializedObject = new SerializedObject(itemData);

            var itemLevelsProperty = serializedObject.FindProperty("itemLevels");

            var changed = false;

            for (int i = 0; i < itemLevelsProperty.arraySize; i++)
            {
                var levelProperty = itemLevelsProperty.GetArrayElementAtIndex(i);
                var priceProperty = levelProperty.FindPropertyRelative("price");

                var currencyId = priceProperty.FindPropertyRelative("currencyId");

                if (string.IsNullOrEmpty(currencyId.stringValue))
                {
                    currencyId.stringValue = defaultCurrencyId;

                    var costProperty = levelProperty.FindPropertyRelative("cost");
                    var amountProperty = priceProperty.FindPropertyRelative("amount");

                    amountProperty.intValue = costProperty.intValue;

                    changed = true;
                }
            }

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if (dataVersionProperty.intValue == 0)
            {
                dataVersionProperty.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(itemData);
            }

            return changed;
        }

        #endregion

        #region UpgradeData

        private static bool TryFixAllUpgradeData(string defaultCurrencyId)
        {
            var guids = AssetDatabase.FindAssets("t:UpgradeData");

            var changed = false;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                UpgradeData upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);

                if (upgrade == null || upgrade.DataVersion == 1) continue;

                if (TryFixUpgradeData(upgrade, defaultCurrencyId))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TryFixUpgradeData(UpgradeData upgradeData, string defaultCurrencyId)
        {
            var serializedObject = new SerializedObject(upgradeData);

            var levelsProperty = serializedObject.FindProperty("levels");

            var changed = false;

            for (int i = 0; i < levelsProperty.arraySize; i++)
            {
                var levelProperty = levelsProperty.GetArrayElementAtIndex(i);
                var priceProperty = levelProperty.FindPropertyRelative("price");

                var currencyId = priceProperty.FindPropertyRelative("currencyId");

                if (string.IsNullOrEmpty(currencyId.stringValue))
                {
                    currencyId.stringValue = defaultCurrencyId;

                    var costProperty = levelProperty.FindPropertyRelative("cost");
                    var amountProperty = priceProperty.FindPropertyRelative("amount");

                    amountProperty.intValue = costProperty.intValue;

                    changed = true;
                }
            }

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if (dataVersionProperty.intValue == 0)
            {
                dataVersionProperty.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(upgradeData);
            }

            return changed;
        }

        #endregion

        #region StageData

        private static bool TryFixAllStageData(string defaultCurrencyId)
        {
            var guids = AssetDatabase.FindAssets("t:StageData");

            var changed = false;

            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                StageData stage = AssetDatabase.LoadAssetAtPath<StageData>(path);

                if (stage == null || stage.DataVersion == 1) continue;

                if (TryFixStageData(stage, defaultCurrencyId))
                {
                    changed = true;
                }
            }

            return changed;
        }

        private static bool TryFixStageData(StageData stageData, string defaultCurrencyId)
        {
            var serializedObject = new SerializedObject(stageData);

            var roomsProperty = serializedObject.FindProperty("rooms");
            
            var changed = false;

            for (int i = 0; i < roomsProperty.arraySize; i++)
            {
                var roomProperty = roomsProperty.GetArrayElementAtIndex(i);
                var wavesProperty = roomProperty.FindPropertyRelative("waves");

                for (int j = 0; j < wavesProperty.arraySize; j++)
                {
                    var wave = wavesProperty.GetArrayElementAtIndex(j);
                    var rewardsProperty = wave.FindPropertyRelative("rewards");

                    if(rewardsProperty.arraySize == 0)
                    {
                        rewardsProperty.arraySize = 1;

                        var priceProperty = rewardsProperty.GetArrayElementAtIndex(0);

                        var currencyIdProperty = priceProperty.FindPropertyRelative("currencyId");
                        currencyIdProperty.stringValue = defaultCurrencyId;

                        var waveDropGoldProperty = wave.FindPropertyRelative("waveDropGold");
                        var amountProperty = priceProperty.FindPropertyRelative("amount");

                        amountProperty.intValue = waveDropGoldProperty.intValue;

                        changed = true;
                    }
                }
            }

            var dataVersionProperty = serializedObject.FindProperty("dataVersion");

            if (dataVersionProperty.intValue == 0)
            {
                dataVersionProperty.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stageData);
            }

            return changed;
        }

        #endregion
    }
}