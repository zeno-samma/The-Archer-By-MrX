using UnityEditor;
using UnityEngine;

namespace OctoberStudio.Armory
{
    public class CloneHeroWindow : EditorWindow
    {
        protected HeroData heroToClone;
        protected string heroName = string.Empty;

        public static void Show(HeroData heroToClone)
        {
            var window = GetWindow<CloneHeroWindow>("Character Cloner Tool");
            window.heroToClone = heroToClone;
        }

        protected virtual void OnGUI()
        {
            EditorGUILayout.HelpBox("Enter the name of the Hero you want to create. The tool will create a new Hero Data asset, Hero prefab and assign appropriate references", MessageType.Info);
            heroName = EditorGUILayout.TextField(heroName);
            GUI.enabled = heroName != string.Empty;
            if(GUILayout.Button("Clone Hero"))
            {
                HeroEditorUtilities.CloneHero(heroToClone, heroName);
                Close();
            }
            GUI.enabled = true;
        }
    }
}