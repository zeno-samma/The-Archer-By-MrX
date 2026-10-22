using UnityEngine;
namespace MrX.Development
{
    public enum GameFeature { Energy = 0, Online = 1, CloudSave = 2, OnlineLeaderboards = 3 }
    [CreateAssetMenu(menuName = "MrX Studio/Game Feature Settings")]
    public sealed class GameFeatureSettings : ScriptableObject
    {
        public const string ResourcePath = "Game Feature Settings";
        [Header("Offline gameplay")]
        public bool energyEnabled;
        [Header("Reserved switches — services are not implemented")]
        public bool onlineEnabled;
        public bool cloudSaveEnabled;
        public bool onlineLeaderboardsEnabled;
        public static GameFeatureSettings Current => Resources.Load<GameFeatureSettings>(ResourcePath);
        public static bool IsEnabled(GameFeature feature)
        {
            var config = Current;
            if (config == null) return feature == GameFeature.Energy; // Preserve existing projects without this asset.
            switch (feature)
            {
                case GameFeature.Energy: return config.energyEnabled;
                case GameFeature.Online: return config.onlineEnabled;
                case GameFeature.CloudSave: return config.onlineEnabled && config.cloudSaveEnabled;
                case GameFeature.OnlineLeaderboards: return config.onlineEnabled && config.onlineLeaderboardsEnabled;
                default: return false;
            }
        }
    }
}
