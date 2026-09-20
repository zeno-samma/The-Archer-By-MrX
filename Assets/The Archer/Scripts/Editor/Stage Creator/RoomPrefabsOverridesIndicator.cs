using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OctoberStudio.StageCreator
{
    public class RoomPrefabsOverridesIndicator
    {
        protected RoomPrefabsHandler RoomPrefabsHandler { get; set; }

        protected GUIContent iconContent = EditorGUIUtility.IconContent("console.warnicon");

        public RoomPrefabsOverridesIndicator(RoomPrefabsHandler roomPrefabsHandler)
        {
            RoomPrefabsHandler = roomPrefabsHandler;
        }

        public virtual void Draw()
        {
            if (RoomPrefabsHandler.HasOverridenPrefabs)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label(iconContent, GUILayout.Width(18), GUILayout.Height(18));

                GUILayout.Label("Overrides", EditorStyles.boldLabel);

                if (GUILayout.Button( "Details", EditorStyles.popup, GUILayout.Width(60)))
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    PopupWindow.Show(
                        new Rect(rect.x + 210, rect.yMax + 100, 1, 1),
                        new PrefabOverridePopup(RoomPrefabsHandler)
                    );
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}