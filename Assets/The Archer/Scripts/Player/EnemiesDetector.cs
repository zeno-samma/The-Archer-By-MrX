using MrX.Easing;
using MrX.Enemy;
using MrX.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace MrX.Player
{
    [DefaultExecutionOrder(5)]
    public class EnemiesDetector : MonoBehaviour
    {
        [SerializeField] protected SphereCollider trigger;
        [SerializeField] protected SpriteRenderer rangeIndicator;

        protected List<EnemyBehavior> detectedEnemies = new List<EnemyBehavior>();

        public bool HasDetectedEnemies => detectedEnemies.Count > 0;

        public float InitialRadius { get; protected set; }
        protected float initialIndicatorAlpha;

        protected IEasingCoroutine indicatorAlphaCoroutine;

        protected Quaternion rangeRotation;
        private int obstacleMask;

        protected virtual void Awake()
        {
            obstacleMask = LayerMask.GetMask("Obstacle");
            initialIndicatorAlpha = rangeIndicator.color.a;

            InitialRadius = trigger.radius;
            if (StageController.Player.Weapon.WeaponData.IsMelee) InitialRadius = StageController.Player.Weapon.WeaponData.AttackReach;
            trigger.radius = InitialRadius;
            rangeIndicator.size = Vector2.one * InitialRadius * 2;
            rangeIndicator.gameObject.SetActive(false);

            rangeRotation = rangeIndicator.transform.rotation;

            StageController.Player.RegisterEnemiesDetector(this);
            StageController.Player.Stats.ArrowRangeStat.onStatChanged += OnRangeChanged;
        }

        protected virtual void LateUpdate()
        {
            if (rangeIndicator.gameObject.activeSelf)
            {
                rangeIndicator.transform.rotation = rangeRotation;
            }
        }

        protected virtual void OnRangeChanged(MultiplicativeStat rangeStat)
        {
            trigger.radius = Mathf.Max(.1f, rangeStat.Value);
            rangeIndicator.size = Vector2.one * rangeStat * 2;
        }

        public virtual void ShowRangeIndicator()
        {
            rangeIndicator.gameObject.SetActive(true);

            indicatorAlphaCoroutine.StopIfExists();

            rangeIndicator.color = rangeIndicator.color.SetAlpha(0);
            indicatorAlphaCoroutine = rangeIndicator.DoAlpha(initialIndicatorAlpha, 0.3f);
        }

        public virtual void HideRangeIndicator()
        {
            indicatorAlphaCoroutine.StopIfExists();

            indicatorAlphaCoroutine = rangeIndicator.DoAlpha(0, 0.3f).SetOnFinish(() => rangeIndicator.gameObject.SetActive(false));
        }

        public virtual bool GetClosestEnemy(out EnemyBehavior closestEnemy, bool raycast)
        {
            if (detectedEnemies.Count == 0)
            {
                closestEnemy = null;
                return false;
            }

            bool hasActiveEnemy = false;
            for (int i = 0; i < detectedEnemies.Count; i++)
            {
                if (detectedEnemies[i].IsColliderEnabled)
                {
                    hasActiveEnemy = true;
                    break;
                }
            }

            if (!hasActiveEnemy)
            {
                closestEnemy = null;
                return false;
            }

            if (raycast)
            {
                return GetClosestReachableEnemy(out closestEnemy);
            }
            else
            {
                return GetSimpleClosestEnemy(out closestEnemy);
            }
        }

        protected virtual bool GetClosestReachableEnemy(out EnemyBehavior closestEnemy)
        {
            var origin = transform.position;
            var closestDistance = float.MaxValue;
            var fallbackDistance = float.MaxValue;
            EnemyBehavior fallback = null;
            closestEnemy = null;

            for (int i = 0; i < detectedEnemies.Count; i++)
            {
                var enemy = detectedEnemies[i];

                if (enemy == null || !enemy.IsColliderEnabled) continue;

                var squaredDistance = (origin - enemy.Position).sqrMagnitude;
                if (squaredDistance < fallbackDistance)
                {
                    fallbackDistance = squaredDistance;
                    fallback = enemy;
                }
                if (squaredDistance >= closestDistance) continue;
                var distance = Mathf.Sqrt(squaredDistance);
                var direction = transform.DirectionToXZ(enemy.Position);
                var ray = new Ray(origin + Vector3.up, direction);
                if (!Physics.Raycast(ray, distance, obstacleMask))
                {
                    closestEnemy = enemy;
                    closestDistance = squaredDistance;
                }
            }

            if (closestEnemy == null) closestEnemy = fallback;
            return closestEnemy != null;
        }

        protected virtual bool GetSimpleClosestEnemy(out EnemyBehavior closestEnemy)
        {
            var closestDistance = float.MaxValue;
            closestEnemy = null;

            foreach (var enemy in detectedEnemies)
            {
                if (!enemy.IsColliderEnabled) continue;

                var distance = (transform.position - enemy.Position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy != null;
        }

        protected virtual int DistanceComparator(EnemyBehavior enemy1, EnemyBehavior enemy2)
        {
            return Mathf.RoundToInt(((transform.position - enemy1.Position).sqrMagnitude - (transform.position - enemy2.Position).sqrMagnitude) * 100);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var enemy = other.GetComponent<EnemyBehavior>();
            if (enemy != null && !detectedEnemies.Contains(enemy))
            {
                detectedEnemies.Add(enemy);
                enemy.SubscribeOnDefeat(OnEnemyDefeated);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            var enemy = other.GetComponent<EnemyBehavior>();
            if (enemy != null)
            {
                detectedEnemies.Remove(enemy);
                enemy.UnsubscribeOnDefeat(OnEnemyDefeated);
            }
        }

        protected virtual void OnEnemyDefeated(IDefeatable defeatable)
        {
            var enemy = (EnemyBehavior)defeatable;
            if (enemy != null)
            {
                detectedEnemies.Remove(enemy);
                enemy.UnsubscribeOnDefeat(OnEnemyDefeated);
            }
        }

        protected virtual void OnDestroy()
        {
            StageController.Player.Stats.ArrowRangeStat.onStatChanged -= OnRangeChanged;
        }
    }
}
