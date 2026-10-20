using UnityEngine;

namespace MrX.Drop
{
    [System.Serializable]
    public class DropData
    {
        public DropData() { }
        public DropData(DropType type, GameObject prefab, int poolSize)
        {
            dropType = type; this.prefab = prefab; initialPoolSize = poolSize;
            canPickUpManually = true; waveEndAutomaticPickUp = true; roomEndAutomaticPickUp = true;
        }
        [SerializeField] protected DropType dropType;
        public DropType DropType => dropType;

        [SerializeField] protected GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] protected int initialPoolSize = 10;
        public int InitialPoolSize => initialPoolSize;

        [SerializeField] protected bool canPickUpManually;

        [Tooltip("This drop is picked up automatically when the wave is finished")]
        [SerializeField] protected bool waveEndAutomaticPickUp;

        [Tooltip("This drop is picked up automatically when the last wave of the room is finished")]
        [SerializeField] protected bool roomEndAutomaticPickUp;

        public bool CanPickUpManually => canPickUpManually;
        public bool WaveEndAutomaticPickUp => waveEndAutomaticPickUp;
        public bool RoomEndAutomaticPickUp => roomEndAutomaticPickUp;
    }
}
