using OctoberStudio.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace OctoberStudio
{
    public class RoomExitBehavior : MonoBehaviour
    {
        [SerializeField] protected Collider trigger;

        [Space]
        [FormerlySerializedAs("disabledExitVisuals")]
        [SerializeField] protected GameObject closedVisuals;
        [FormerlySerializedAs("enabledExitVisuals")]
        [SerializeField] protected GameObject openedExitVisuals;

        [Space]
        [SerializeField] protected AudioData openSound;

        [Space]
        [SerializeField] protected Image nextRoomPreviewImage;

        public virtual void Init(bool spawnEnabled, Sprite nextRoomPreview = null)
        {
            trigger.enabled = spawnEnabled;

            if (closedVisuals != null) closedVisuals.SetActive(!spawnEnabled);
            if (openedExitVisuals != null) openedExitVisuals.SetActive(spawnEnabled);

            if (nextRoomPreviewImage != null)
            {
                nextRoomPreviewImage.sprite = nextRoomPreview;
                nextRoomPreviewImage.enabled = spawnEnabled && nextRoomPreview != null;
            }
        }

        public virtual void Show()
        {
            trigger.enabled = true;

            if (closedVisuals != null) closedVisuals.SetActive(false);
            if (openedExitVisuals != null) openedExitVisuals.SetActive(true);

            GameController.AudioManager.PlayAudio(openSound);

            if (nextRoomPreviewImage != null && nextRoomPreviewImage.sprite != null)
            {
                nextRoomPreviewImage.enabled = true;
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerBehavior>();

            if (player != null)
            {
                StageController.OnExitReached(this);
            }
        }
    }
}