using System.Collections.Generic;
using OctoberStudio.Armory;
using OctoberStudio.StageCreator;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio
{
    [CustomPropertyDrawer(typeof(WeaponAnimationSetDataList))]
    public class WeaponAnimationSetDataListPropertyDrawer : PropertyDrawer
    {
        protected static float WeaponsRowHeight = 50f;
        protected static float WeaponColumnWidth = 100f;

        private static Vector2 scrollPos;

        private static readonly Dictionary<string, SerializedProperty> propertyCache =
            new Dictionary<string, SerializedProperty>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var serializedObject = property.serializedObject;

            if (!(serializedObject.targetObject is ArmoryDatabase database))
            {
                base.OnGUI(position, property, label);
                return;
            }

            var arrayProperty = property.FindPropertyRelative("weaponAnimationSets");

            BuildCache(arrayProperty);

            float contentWidth = GetWidth(position, property);
            float contentHeight = GetPropertyHeight(property, label) - GUI.skin.horizontalScrollbar.fixedHeight;

            Rect contentRect = new Rect(0, 0, contentWidth, contentHeight);

            EditorGUI.BeginChangeCheck();

            scrollPos = GUI.BeginScrollView(position, scrollPos, contentRect, true, true);

            DrawGrid(database, arrayProperty, contentRect);

            GUI.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void BuildCache(SerializedProperty arrayProperty)
        {
            propertyCache.Clear();

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var item = arrayProperty.GetArrayElementAtIndex(i);

                string heroId =
                    item.FindPropertyRelative("heroId").stringValue;

                string weaponId =
                    item.FindPropertyRelative("weaponId").stringValue;

                string key = heroId + "|" + weaponId;

                if (!propertyCache.ContainsKey(key))
                {
                    propertyCache.Add(key, item);
                }
            }
        }

        private void DrawGrid(ArmoryDatabase database, SerializedProperty arrayProperty, Rect position)
        {
            float xOffset = 100f;

            // -------------------------
            // HEADER (WEAPONS)
            // -------------------------
            int weaponIndex = 0;

            for (int i = 0; i < database.ItemsCount; i++)
            {
                var weapon = database.Items[i];
                if (weapon == null || weapon.ItemType != ItemType.Weapon)
                    continue;

                var rect = new Rect(
                    xOffset + WeaponColumnWidth * weaponIndex,
                    0,
                    WeaponColumnWidth,
                    WeaponsRowHeight
                );

                GUI.Label(rect, weapon.ItemName);

                EditorGUI.DrawRect(
                    new Rect(rect.x, 0, 1, position.height),
                    Color.black
                );

                weaponIndex++;
            }

            EditorGUI.DrawRect(
                new Rect(
                    xOffset + WeaponColumnWidth * weaponIndex,
                    0,
                    1,
                    position.height
                ),
                Color.black
            );

            // -------------------------
            // HERO ROWS
            // -------------------------
            int heroIndex = 0;

            for (int i = 0; i < database.HeroesCount; i++)
            {
                var hero = database.Heroes[i];

                if (hero == null)
                    continue;

                float y =
                    WeaponsRowHeight +
                    heroIndex * (
                        EditorGUIUtility.singleLineHeight +
                        EditorGUIUtility.standardVerticalSpacing
                    );

                GUI.Label(
                    new Rect(
                        0,
                        y,
                        100,
                        EditorGUIUtility.singleLineHeight
                    ),
                    hero.Name
                );

                EditorGUI.DrawRect(
                    new Rect(
                        0,
                        y,
                        position.width,
                        1
                    ),
                    Color.black
                );

                int weaponIndexRow = 0;

                for (int j = 0; j < database.ItemsCount; j++)
                {
                    var weapon = database.Items[j];

                    if (weapon == null || weapon.ItemType != ItemType.Weapon)
                        continue;

                    var dataProperty = GetOrCreateAnimationSetDataProperty(
                        arrayProperty,
                        hero.Id,
                        weapon.Id
                    );

                    var setProperty =
                        dataProperty.FindPropertyRelative("animationsSet");

                    var cellRect = new Rect(
                        100 + WeaponColumnWidth * weaponIndexRow,
                        y,
                        WeaponColumnWidth,
                        EditorGUIUtility.singleLineHeight
                    );

                    cellRect = cellRect.Shrink(2f);

                    setProperty.objectReferenceValue =
                        EditorGUI.ObjectField(
                            cellRect,
                            setProperty.objectReferenceValue,
                            typeof(WeaponAnimationsSet),
                            false
                        );

                    weaponIndexRow++;
                }

                heroIndex++;
            }
        }

        protected SerializedProperty GetOrCreateAnimationSetDataProperty(
            SerializedProperty arrayProperty,
            string heroId,
            string weaponId)
        {
            string key = heroId + "|" + weaponId;

            if (propertyCache.TryGetValue(key, out var dataProperty))
            {
                return dataProperty;
            }

            arrayProperty.arraySize++;

            dataProperty =
                arrayProperty.GetArrayElementAtIndex(
                    arrayProperty.arraySize - 1
                );

            dataProperty.FindPropertyRelative("weaponId").stringValue =
                weaponId;

            dataProperty.FindPropertyRelative("heroId").stringValue =
                heroId;

            dataProperty.FindPropertyRelative("animationsSet")
                .objectReferenceValue = null;

            propertyCache[key] = dataProperty;

            return dataProperty;
        }

        protected virtual float GetWidth(Rect position, SerializedProperty property)
        {
            var serializedObject = property.serializedObject;

            if (serializedObject.targetObject is ArmoryDatabase database)
            {
                int weaponsCount = 0;

                for (int i = 0; i < database.ItemsCount; i++)
                {
                    var weapon = database.Items[i];

                    if (weapon != null &&
                        weapon.ItemType == ItemType.Weapon)
                    {
                        weaponsCount++;
                    }
                }

                return 100 + weaponsCount * WeaponColumnWidth;
            }

            return position.width;
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            var serializedObject = property.serializedObject;

            if (serializedObject.targetObject is ArmoryDatabase database)
            {
                return database.HeroesCount *
                       (
                           EditorGUIUtility.singleLineHeight +
                           EditorGUIUtility.standardVerticalSpacing
                       ) +
                       WeaponsRowHeight + GUI.skin.horizontalScrollbar.fixedHeight;
            }

            return base.GetPropertyHeight(property, label);
        }
    }
}