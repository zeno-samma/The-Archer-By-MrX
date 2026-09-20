using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Drop
{
    public class DropBehavior : MonoBehaviour
    {
        [SerializeField] protected GameObject pickUpParticlePrefab;
        [SerializeField] protected AudioData pickUpSound;
        [SerializeField] protected AudioData spawnSound;
        [SerializeField] protected Collider trigger;

        [Space]
        [SerializeField] protected TrailRenderer trail;

        [Space]
        [Tooltip("Amount of time after which the drop will despawn. 0 = endless lifetime. XP Gem cannot have lifetime")]
        [SerializeField] protected float lifetime = 0;

        [Header("Spawn Animation")]
        [SerializeField] protected Float scaleDuration = (0.2f, 0.25f);
        [SerializeField] protected Float finalScale = (0.9f, 1f);
        [Space]
        [SerializeField] protected Float distance = (0.5f, 1f);
        [SerializeField] protected Float height = (1f, 1.2f);
        [SerializeField] protected Float spawnDuration = (0.4f, 0.6f);
        [SerializeField] protected AnimationCurve distanceCurve;
        [SerializeField] protected AnimationCurve heightCurve;

        protected DropData dropData;
        public DropData DropData => dropData;

        public DropType DropType => dropData.DropType;

        public event UnityAction<DropBehavior> onPickedUp;
        public event UnityAction<DropBehavior> onDespawning;

        protected IEasingCoroutine scaleCoroutine;
        protected IEasingCoroutine spawnCoroutine;
        protected IEasingCoroutine spawnPositionCoroutine;
        protected IEasingCoroutine despawnCoroutine;
        protected IEasingCoroutine flyCoroutine;

        public bool IsSpawning { get; protected set; } = false;
        public bool IsFlyingToPlayer { get; protected set; }

        public float SpawnTime { get; protected set; }

        protected virtual void Awake()
        {
            if (pickUpParticlePrefab != null)
            {
                StageController.ParticlesManager.RegisterParticle(pickUpParticlePrefab);
            }
        }

        public virtual void Init(DropData dropData, float spawnDelay)
        {
            this.dropData = dropData;

            if(trail != null) trail.Clear();

            transform.localScale = Vector3.zero;

            if(trigger != null) trigger.enabled = false;

            IsSpawning = true;
            EasingManager.DoAfter(spawnDelay, Spawn);
        }

        public virtual void Spawn()
        {
            scaleCoroutine = transform.DoLocalScale(Vector3.one * finalScale, scaleDuration).SetEasing(EasingType.QuadOut);

            var spawnPosition = transform.position;
            var endPosition = transform.position + Quaternion.Euler(0, Random.Range(-180f, 180f), 0) * Vector3.forward * distance;
            var duration = spawnDuration.Value;

            spawnCoroutine = EasingManager.DoFloat(0, 1, duration, (t) => { 
                var position = Vector3.Lerp(spawnPosition, endPosition, t);
                position.y = transform.position.y;
                transform.position = position;
            }).SetEasingCurve(distanceCurve);

            var height = this.height.Value;
            spawnPositionCoroutine = EasingManager.DoFloat(0, 1, duration, (t) => {
                var y = t * height;
                var position = transform.position;
                position.y = y;
                transform.position = position;
            }).SetEasingCurve(heightCurve).SetOnFinish(() => {
                transform.position = endPosition;
                IsSpawning = false;
                if(trigger != null) trigger.enabled = true;
                SpawnTime = Time.time;
            });

            if (spawnSound != null)
            {
                GameController.AudioManager.PlayAudio(spawnSound);
            }

            IsFlyingToPlayer = false;
        }

        protected virtual void Update()
        {
            if (lifetime <= 0) return;
            if (IsFlyingToPlayer) return;
            if (IsSpawning) return;
            if (despawnCoroutine.ExistsAndActive()) return;
            if (DropData.DropType == DropType.XPGem) return;

            if(lifetime > 0 && Time.time > SpawnTime + lifetime)
            {
                onDespawning?.Invoke(this);

                if (trigger != null) trigger.enabled = false;

                despawnCoroutine = transform.DoLocalScale(Vector3.zero, 0.3f).SetEasing(EasingType.QuadIn).SetOnFinish(() => gameObject.SetActive(false));
            }
        }

        public virtual void FlyToPlayer(Transform playerTransform, AnimationCurve pickUpEasingCurve)
        {
            IsFlyingToPlayer = true;

            if (spawnCoroutine.ExistsAndActive())
            {
                StartCoroutine(WaitAndFlyToPlayer(playerTransform, pickUpEasingCurve));
            } else
            {
                flyCoroutine.StopIfExists();
                flyCoroutine = transform.DoPosition(StageController.Player.transform, 0.4f).SetEasingCurve(pickUpEasingCurve).SetOnFinish(() =>
                {
                    OnPickedUp();
                });
            }
        }

        protected virtual IEnumerator WaitAndFlyToPlayer(Transform playerTransform, AnimationCurve pickUpEasingCurve)
        {
            yield return new WaitUntil(() => spawnCoroutine.IsActive);

            flyCoroutine.StopIfExists();
            flyCoroutine = transform.DoPosition(StageController.Player.transform, 0.4f).SetEasingCurve(pickUpEasingCurve).SetOnFinish(() =>
            {
                OnPickedUp();
            });
        }

        public virtual void OnPickedUp()
        {
            if(pickUpParticlePrefab != null)
            {
                var particle = StageController.ParticlesManager.GetParticle(pickUpParticlePrefab);

                particle.transform.position = transform.position;
                particle.Play();
            }

            if (pickUpSound != null)
            {
                GameController.AudioManager.PlayAudio(pickUpSound);
            }

            IsFlyingToPlayer = false;

            onPickedUp?.Invoke(this);

            if (trail != null) trail.Clear();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != StageController.Player.gameObject) return;
            if (!dropData.CanPickUpManually) return;

            OnPickedUp();
            gameObject.SetActive(false);
        }

        protected virtual void OnDisable()
        {
            scaleCoroutine.StopIfExists();
            flyCoroutine.StopIfExists();
            spawnPositionCoroutine.StopIfExists();
            spawnCoroutine.StopIfExists();
            despawnCoroutine.StopIfExists();
        }
    }
}