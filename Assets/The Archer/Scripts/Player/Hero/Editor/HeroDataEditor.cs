using MrX.Armory;
using UnityEditor;
using UnityEngine;

namespace MrX
{
    [CustomEditor(typeof(HeroData), true)]
    public class HeroDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var hero = target as HeroData;
            if (hero.Prefab == null) return;

            GUILayout.Space(10f);

            if (GUILayout.Button("Clone Hero", GUILayout.Width(100)))
            {
                CloneHeroWindow.Show(hero);
            }
        }
    }
}