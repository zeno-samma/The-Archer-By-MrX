using MrX.UI;
using UnityEditor;
using UnityEngine;

namespace MrX.EditorTools
{
    public static class ArmoryDashboardBuilder
    {
        public const string Path = "Assets/The Archer/Resources/Armory Dashboard Database.asset";
        [MenuItem("Tools/MrX Studio/Armory/Create Dashboard Settings If Missing")]
        public static void Create()
        {
            var data = AssetDatabase.LoadAssetAtPath<ArmoryDashboardDatabase>(Path);
            if (data != null) { Selection.activeObject = data; return; }
            var forge = Resources.Load<ForgePageDatabase>(ForgePageDatabase.ResourcePath);
            data = ScriptableObject.CreateInstance<ArmoryDashboardDatabase>();
            data.slotSprite = forge.slotSprite; data.slotMaterial = forge.slotMaterial; data.font = forge.font;
            data.emptySlotIcon = Sprite("Armory/ui_slot_weapon.png"); data.lockIcon = Sprite("Generic/ui_icon_lock.png");
            data.blueButton = Sprite("Generic/ui_button_blue.png"); data.yellowButton = forge.forgeButtonSprite; data.grayButton = forge.disabledButtonSprite;
            AssetDatabase.CreateAsset(data, Path); AssetDatabase.SaveAssets(); Selection.activeObject = data;
        }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/The Archer/Sprites/UI/" + path);
    }
}
