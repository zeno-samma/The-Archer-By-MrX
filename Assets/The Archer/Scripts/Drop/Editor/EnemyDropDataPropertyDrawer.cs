using OctoberStudio.StageCreator;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OctoberStudio.Drop
{
    [CustomPropertyDrawer(typeof(EnemyDropData))]
    public class EnemyDropDataPropertyDrawer : PropertyDrawer
    {
        protected GUIStyle textStyle;

        protected virtual void OnItemDropGUI(Rect position, SerializedProperty property)
        {
            var itemDataProperty = property.FindPropertyRelative("itemData");
            var itemDataPosition = new Rect(position);
            itemDataPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(itemDataPosition, itemDataProperty);

            var hpZonesProperty = property.FindPropertyRelative("hpZones");
            hpZonesProperty.arraySize = 1;

            var zoneProperty = hpZonesProperty.GetArrayElementAtIndex(0);
            ((Int)1).SaveToSerializedProperty(zoneProperty.FindPropertyRelative("dropAmount"));
            zoneProperty.FindPropertyRelative("chance").floatValue = 100f;
        }

        protected virtual void OnZonesCountGUI(Rect position, SerializedProperty hpZonesProperty, SerializedProperty selectedPropertyId)
        {
            var chanceRect = new Rect(position);
            chanceRect.y += EditorGUIUtility.standardVerticalSpacing;
            chanceRect.height = EditorGUIUtility.singleLineHeight;
            chanceRect.x += 17f;

            var labelRect = new Rect(chanceRect);
            labelRect.width = 100f;

            EditorGUI.LabelField(labelRect, "HP Drop Zones", EditorStyles.boldLabel);

            if (hpZonesProperty.arraySize == 0) hpZonesProperty.arraySize = 1;
            var zonesCount = hpZonesProperty.arraySize;

            var buttonsRect = new Rect(chanceRect);
            buttonsRect.x += buttonsRect.width - 16 * 5 - 2 * 4 - 17;
            buttonsRect.width = 16 * 4 - 2 * 3;

            zonesCount = DrawZonesCountSelector(buttonsRect, 16, 2, hpZonesProperty.arraySize);
            bool recalculate = zonesCount > hpZonesProperty.arraySize;
            if (zonesCount > hpZonesProperty.arraySize)
            {
                if (hpZonesProperty.arraySize < zonesCount) hpZonesProperty.arraySize = zonesCount;
            }

            if (hpZonesProperty.arraySize != zonesCount) hpZonesProperty.arraySize = zonesCount;
            if (recalculate) RecalculateZones(hpZonesProperty, zonesCount);

            if (zonesCount <= selectedPropertyId.intValue) selectedPropertyId.intValue = zonesCount - 1;
        }

        protected virtual void OnNotItemDropGUI(Rect position, SerializedProperty property)
        {
            var hpZonesProperty = property.FindPropertyRelative("hpZones");
            var selectedPropertyId = property.FindPropertyRelative("selectedZoneId");

            OnZonesCountGUI(position, hpZonesProperty, selectedPropertyId);

            var chancesRect = new Rect(position);
            chancesRect.y += 16 + EditorGUIUtility.standardVerticalSpacing;
            chancesRect.height = 20;

            chancesRect.x += 17;
            chancesRect.width -= 34;

            EditorGUI.DrawRect(chancesRect, new Color(0.1f, 0.1f, 0.1f, 1));

            int count = hpZonesProperty.arraySize;
            float step = chancesRect.width / count;
            var valueSum = 0f;
            for (int i = 0; i < count; i++)
            {
                var zoneProperty = hpZonesProperty.GetArrayElementAtIndex(i);
                var hpProperty = zoneProperty.FindPropertyRelative("hp");

                var value = hpProperty.floatValue;
                if (value < 0.15f) value = 0.15f;
                if (i == count - 1)
                {
                    value = 1 - valueSum;
                }

                var start = valueSum;
                var end = start + value;

                if (i != count - 1)
                {
                    var handleRect = chancesRect.Shrink(1);
                    handleRect.x += handleRect.width * end - 3;
                    handleRect.width = 6;

                    EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);

                    if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = i + 1000;
                        Event.current.Use();
                    }
                    if (GUIUtility.hotControl == i + 1000 && Event.current.type == EventType.MouseDrag)
                    {
                        var nextZoneProperty = hpZonesProperty.GetArrayElementAtIndex(i + 1);
                        var nextHPProperty = nextZoneProperty.FindPropertyRelative("hp");
                        var nextValue = nextHPProperty.floatValue;

                        var newValue = Mathf.Clamp01((Event.current.mousePosition.x - chancesRect.x) / chancesRect.width) - start;

                        if (newValue < value)
                        {
                            if (newValue < 0.15f) newValue = 0.15f;

                            var difference = value - newValue;
                            value = newValue;
                            nextValue += difference;

                        }
                        else
                        {
                            var difference = newValue - value;
                            if (nextValue - difference > 0.15f)
                            {
                                nextValue -= difference;
                                value = newValue;
                            }
                            else
                            {
                                difference = nextValue - 0.15f;

                                nextValue = 0.15f;
                                value += difference;

                            }
                        }
                        nextHPProperty.floatValue = nextValue;
                        hpProperty.floatValue = value;

                        end = start + value;
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == i + 1000)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                }
                else
                {
                    hpProperty.floatValue = value;
                }

                valueSum += value;

                var subRect = chancesRect.Shrink(1);
                subRect.x += subRect.width * start + 1;
                subRect.width = subRect.width * (end - start) - 2;

                var selectRect = subRect.Shrink(4, 2);
                EditorGUIUtility.AddCursorRect(selectRect, MouseCursor.Link);

                if (selectRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        selectedPropertyId.intValue = i;

                        EditorGUI.FocusTextInControl(null);
                    }
                }

                var color = Color.Lerp(Color.red, Color.green, count > 1 ? i / (count - 1f) : 1) * 0.9f;
                color.a = 1;

                if (selectedPropertyId.intValue == i)
                {
                    EditorGUI.DrawRect(subRect, Color.blue);
                    EditorGUI.DrawRect(subRect.Shrink(1), color);
                }
                else
                {
                    EditorGUI.DrawRect(subRect, color);
                }

                var labelRect = new Rect(subRect);
                labelRect.y += subRect.height + 2;
                labelRect.width = 34;
                labelRect.height = 12;
                labelRect.x -= 17;

                var labelPersent = Mathf.Round(start * 100).ToString() + "%";
                EditorGUI.LabelField(labelRect, new GUIContent(labelPersent), textStyle);
            }

            var lastLabelRect = new Rect();
            lastLabelRect.x = chancesRect.x + chancesRect.width - 17;
            lastLabelRect.y = chancesRect.y + chancesRect.height;
            lastLabelRect.height = 12;
            lastLabelRect.width = 34;

            EditorGUI.LabelField(lastLabelRect, new GUIContent("100%"), textStyle);

            var selectedZone = hpZonesProperty.GetArrayElementAtIndex(selectedPropertyId.intValue);
            DrawZone(chancesRect, selectedZone);
        }

        public virtual void DrawZone(Rect position, SerializedProperty zoneProperty)
        {
            position.y += position.height + 12 + EditorGUIUtility.standardVerticalSpacing * 2;

            var dropAmountProperty = zoneProperty.FindPropertyRelative("dropAmount");
            var chanceProperty = zoneProperty.FindPropertyRelative("chance");
            EditorGUI.PropertyField(position, dropAmountProperty);

            position.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, chanceProperty);
            //EditorGUI.DrawRect(position, Color.black);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var dropTypeProperty = property.FindPropertyRelative("dropType");
            var dropTypePosition = new Rect(position);
            dropTypePosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(dropTypePosition, dropTypeProperty);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            position.height -= EditorGUIUtility.singleLineHeight;

            var dropType = (DropType) dropTypeProperty.intValue;
            if (dropType == DropType.Item) 
            {
                OnItemDropGUI(position, property);
            } else
            {
                OnNotItemDropGUI(position, property);
            }

            EditorGUI.EndProperty();
        }

        protected virtual void RecalculateZones(SerializedProperty hpZonesProperty, int count)
        {
            var sumValue = 0f;
            for (int i = 0; i < count; i++)
            {
                var zoneProperty = hpZonesProperty.GetArrayElementAtIndex(i);
                var hpProperty = zoneProperty.FindPropertyRelative("hp");

                var zoneValue = hpProperty.floatValue;
                if (zoneValue + sumValue > 1) zoneValue = 1 - sumValue;

                hpProperty.floatValue = zoneValue;
                sumValue += zoneValue;
            }

            for (int i = count - 1; i > 0; i--)
            {
                var zoneProperty = hpZonesProperty.GetArrayElementAtIndex(i);
                var hpProperty = zoneProperty.FindPropertyRelative("hp");

                var zoneValue = hpProperty.floatValue;
                if (zoneValue >= 0.15f) continue;

                var difference = 0.15f - zoneValue;

                zoneValue = 0.15f;
                hpProperty.floatValue = zoneValue;

                var prevZoneProperty = hpZonesProperty.GetArrayElementAtIndex(i - 1);
                var prevHPProperty = prevZoneProperty.FindPropertyRelative("hp");
                prevHPProperty.floatValue = prevHPProperty.floatValue - difference;
            }
        }

        protected virtual int DrawZonesCountSelector(Rect position, int size, int spacing, int selectedId)
        {
            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label);
                textStyle.alignment = TextAnchor.MiddleCenter;
            }

            for (int i = 1; i <= 4; i++)
            {
                var rect = new Rect(position);
                rect.x += (size + spacing) * (i - 1);
                rect.width = size;
                rect.height = size;

                EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));

                if(selectedId == i)
                {
                    EditorGUI.DrawRect(rect.Shrink(1), new Color(0.3f, 0.3f, 0.3f, 1f));
                } else
                {
                    EditorGUI.DrawRect(rect.Shrink(1), new Color(0.2f, 0.2f, 0.2f, 1f));
                }

                EditorGUI.LabelField(rect, i.ToString(), textStyle);

                if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                {
                    selectedId = i;
                }

                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            }

            return selectedId;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var dropTypeProperty = property.FindPropertyRelative("dropType");
            if(dropTypeProperty.intValue == (int)DropType.Item)
            {
                return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3 + 54;
            }

        }
    }
}