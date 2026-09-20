using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class WaspChargeLineBehavior : MonoBehaviour
    {
        [SerializeField] protected SpriteRenderer chargeLineSprite;

        protected float distance;
        protected float duration;

        protected float time = 0;
        protected bool isGrowing;

        public Vector3 EndPosition { get; protected set; }
        protected float width;

        protected virtual void Awake()
        {
            width = chargeLineSprite.size.x;
        }

        public virtual void Show(float distance, float duration)
        {
            gameObject.SetActive(true);

            this.distance = distance;
            this.duration = duration;

            time = 0;
            isGrowing = true;

            chargeLineSprite.size = new Vector2(width, 0);
        }

        protected virtual void Update()
        {
            if (isGrowing)
            {
                time += Time.deltaTime;
                if (time >= duration)
                {
                    isGrowing = false;
                    time = duration;
                }
            }

            var t = time / duration;

            EndPosition = transform.position + transform.forward * t * distance;
            if (!StageController.NavigationManager.IsStraightPathAvailable(transform.position, EndPosition, out var obstaclePosition, out var hitNormal))
            {
                EndPosition = obstaclePosition;
            }

            var localDistance = Vector3.Distance(transform.position, EndPosition);

            chargeLineSprite.size = new Vector2(width, localDistance);
            chargeLineSprite.transform.position = transform.position + transform.forward * localDistance / 2;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}