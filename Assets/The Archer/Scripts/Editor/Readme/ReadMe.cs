using UnityEngine;

namespace OctoberStudio.ReadMe
{
    [CreateAssetMenu]
    public class ReadMe : ScriptableObject
    {
        [SerializeField] protected Texture readmeBanner;
        [SerializeField] protected string description;
        [SerializeField] protected string urpDescription;

        [SerializeField] protected string documentationURL;
        [SerializeField] protected string discordURL;
        [SerializeField] protected string assetURL;

        [SerializeField] protected string email;

        public Texture ReadmeBanner => readmeBanner;
        public string Description => description;
        public string URPDescription => urpDescription;

        public string DocumentationURL => documentationURL;
        public string DiscordURL => discordURL;
        public string AssetURL => assetURL;

        public string Email => email;
    }
}