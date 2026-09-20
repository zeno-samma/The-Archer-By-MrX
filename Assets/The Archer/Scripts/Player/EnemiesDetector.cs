using OctoberStudio.Easing;
using OctoberStudio.Enemy;
using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Player
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

        protected virtual void Awake()
        {
            initialIndicatorAlpha = rangeIndicator.color.a;

            InitialRadius = trigger.radius;
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
            detectedEnemies.Sort(DistanceComparator);

            var obstacleMask = LayerMask.GetMask("Obstacle");

            for (int i = 0; i < detectedEnemies.Count; i++)
            {
                var enemy = detectedEnemies[i];

                if (!enemy.IsColliderEnabled) continue;

                var distance = Vector3.Distance(transform.position, enemy.Position);
                var direction = transform.DirectionToXZ(detectedEnemies[i].Position);
                var ray = new Ray(transform.position + Vector3.up, direction);
                if (!Physics.Raycast(ray, distance, obstacleMask))
                {
                    closestEnemy = enemy;
                    return true;
                }
            }

            closestEnemy = detectedEnemies[0];
            return true;
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