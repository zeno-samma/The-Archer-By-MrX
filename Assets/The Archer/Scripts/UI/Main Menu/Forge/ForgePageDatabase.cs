using MrX.Armory;
using TMPro;
using UnityEngine;

namespace MrX.UI
{
    [CreateAssetMenu(menuName = "MrX Studio/Forge/Page Database", fileName = "Forge Page Database")]
    public class ForgePageDatabase : ScriptableObject
    {
        public const string ResourcePath = "Forge Page Database";
        public ArmoryDatabase armory;
        public Sprite slotSprite;
        public Material slotMaterial;
        public Sprite forgeIcon;
        public Sprite coinIcon;
        public Sprite settingsIcon;
        public Sprite forgeButtonSprite;
        public Sprite disabledButtonSprite;
        public Sprite circleSprite;
        public TMP_FontAsset font;
        [Tooltip("Optional illustration. Existing hammer art is used when empty.")]
        public Sprite anvilIllustration;
    }
}
