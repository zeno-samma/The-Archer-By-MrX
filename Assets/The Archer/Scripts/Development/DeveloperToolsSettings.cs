using MrX.Armory;
using UnityEngine;

namespace MrX.Development
{
    [CreateAssetMenu(menuName = "MrX Studio/Developer Tools Settings")]
    public sealed class DeveloperToolsSettings : ScriptableObject
    {
        public bool toolsEnabled = true;
        public bool allowInDevelopmentBuilds = true;
        public bool allowInReleaseBuilds;
        public ArmoryDatabase armory;
        public int[] currencyAmounts = { 100, 1000, 10000, 100000 };
        public event System.Action Changed;
        public bool Allows(bool editor, bool developmentBuild) => toolsEnabled &&
            (editor || (developmentBuild ? allowInDevelopmentBuilds : allowInReleaseBuilds));
        public bool IsAvailable => Allows(Application.isEditor, Debug.isDebugBuild);
        private void OnValidate() => Changed?.Invoke();
    }
}
