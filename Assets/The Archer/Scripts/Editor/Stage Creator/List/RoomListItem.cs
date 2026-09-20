using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class RoomListItem : ListItem
    {
        protected static readonly float defaultHeight = 40f;
        protected static readonly float smallOffset = 2f;
        protected static readonly float borderOffset = 1.5f;
        protected static readonly float textureOffset = 5f;
        protected static readonly float dragOffset = 10f;

        protected static GUIStyle textStyle;

        public SerializedProperty RoomProperty { get; protected set; }
        protected override float DefaultHeight => defaultHeight;

        public override bool EqualsTo(SerializedProperty property)
        {
            return property.propertyPath == RoomProperty.propertyPath;
        }

        public override float GetHeight()
        {
            return defaultHeight;
        }

        public override void Draw()
        {
            if(textStyle == null) textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };

            if (Event.current.type == EventType.ContextClick && HeaderRect.Contains(Event.current.mousePosition))
            {
                StageCreatorWindow.Instance.ContextManager.ShowContextMenu(this);
                Event.current.Use();
            }

            var rect = new Rect(FullRect);

            if (isPickedUp || isSelected)
            {
                if (isPickedUp)
                    rect.y += Event.current.mousePosition.y - SelectMousePosition;

                EditorGUI.DrawRect(rect, new Color(0.05f, 0.05f, 0.05f));
                EditorGUI.DrawRect(GetFillRect(rect), new Color(0.3f, 0.3f, 0.3f));
            }
            else
            {
                EditorGUI.DrawRect(rect, new Color(0.05f, 0.05f, 0.05f));
                EditorGUI.DrawRect(GetFillRect(rect), new Color(0.15f, 0.15f, 0.15f));
            }

            var groupId = RoomProperty.FindPropertyRelative("groupId").intValue;

            if(groupId > 0)
            {
                var color = StageCreatorWindow.Instance.StageDatabase.GetRandomGroupColor(groupId - 1);
                var groupRect = GetGroupRect(rect);

                GUI.DrawTexture(groupRect, StageCreatorWindow.Instance.CircleTexture, ScaleMode.ScaleToFit, true, 0, color, 0, 0);
                var style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 16,
                };
                EditorGUI.LabelField(groupRect, groupId.ToString(), style);

            }

            EditorGUI.LabelField(GetRoomNumberRect(rect), $"Room {Index + 1:D3}", textStyle);

            GUI.DrawTexture(GetDragHandleRect(rect), StageCreatorWindow.Instance.DragHandleTexture);
        }

        public override void Init(EditorWindow window, SerializedProperty property, int index)
        {
            Index = index;

            Window = window;

            RoomProperty = property;
        }

        public RoomData RoomDataCache { get; protected set; }

        public override void CacheData()
        {
            RoomDataCache = RoomProperty.GetValue<RoomData>();
        }

        public override void WriteToProperty(SerializedProperty property)
        {
            RoomDataCache.SaveToSerializedProperty(property);
        }

        public virtual Rect GetFillRect(Rect mainRect)
        {
            var fillRect = new Rect(mainRect);
            fillRect.x += borderOffset;
            fillRect.y += borderOffset;
            fillRect.width -= borderOffset * 2;
            fillRect.height -= borderOffset * 2;
            return fillRect;
        }

        public Rect GetDragHandleRect(Rect mainRect)
        {
            var handleRect = new Rect(mainRect);

            handleRect.x += smallOffset;
            handleRect.y += dragOffset;
            handleRect.width = defaultHeight - dragOffset * 2;
            handleRect.height = defaultHeight - dragOffset * 2;

            return handleRect;
        }

        public virtual Rect GetGroupRect(Rect mainRect)
        {
            var groupRect = new Rect(mainRect);

            groupRect.x += groupRect.width - defaultHeight;
            groupRect.y += textureOffset;
            groupRect.width = defaultHeight - textureOffset * 2;
            groupRect.height = defaultHeight - textureOffset * 2;

            return groupRect;
        }

        public virtual Rect GetRoomNumberRect(Rect mainRect)
        {
            var stageNumberRect = new Rect(mainRect);

            var offset = defaultHeight - 10 * 2;

            stageNumberRect.x += offset;
            stageNumberRect.y += defaultHeight / 2 - EditorGUIUtility.singleLineHeight / 2;
            stageNumberRect.width -= defaultHeight;
            stageNumberRect.height = defaultHeight / 2 - smallOffset * 2;

            return stageNumberRect;
        }

        public virtual float GetAllExperience()
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");

            var xp = 0f;
            for(int i = 0; i < wavesProperty.arraySize; i++)
            {
                var waveProperty = wavesProperty.GetArrayElementAtIndex(i);
                var waveData = waveProperty.GetValue<WaveData>();
                xp += waveData.WaveDropExperience;
            }

            return xp;
        }

        public virtual int GetAllCurrency(string currencyId)
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");

            var amount = 0;
            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                var waveProperty = wavesProperty.GetArrayElementAtIndex(i);
                var wavedata = waveProperty.GetValue<WaveData>();

                foreach(var reward in wavedata.Rewards)
                {
                    if(reward.CurrencyId == currencyId)
                    {
                        amount += reward.Amount;
                        break;
                    }
                }
            }

            return amount;
        }

        public virtual float GetAllExperienceUpTo(SerializedProperty currentWave)
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");

            var xp = 0f;
            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                var waveProperty = wavesProperty.GetArrayElementAtIndex(i);

                var waveData = waveProperty.GetValue<WaveData>();
                xp += waveData.WaveDropExperience;

                if (waveProperty.propertyPath == currentWave.propertyPath) break;
            }

            return xp;
        }

        public virtual int GetAllCurrencyUpTo(SerializedProperty currentWave, string currencyId)
        {
            var wavesProperty = RoomProperty.FindPropertyRelative("waves");

            var amount = 0;
            for (int i = 0; i < wavesProperty.arraySize; i++)
            {
                var waveProperty = wavesProperty.GetArrayElementAtIndex(i);
                var wavedata = waveProperty.GetValue<WaveData>();

                foreach (var reward in wavedata.Rewards)
                {
                    if (reward.CurrencyId == currencyId)
                    {
                        amount += reward.Amount;
                        break;
                    }
                }

                if (waveProperty.propertyPath == currentWave.propertyPath) break;
            }

            return amount;
        }

        public virtual void PasteValuesFrom(RoomListItem other)
        {
            var roomData = other.RoomProperty.GetValue<RoomData>();
            roomData.SaveToSerializedProperty(RoomProperty);

            RoomProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}