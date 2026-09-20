using OctoberStudio.Enemy;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class TargetRingBehavior : MonoBehaviour
    {
        public static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");
        public static readonly int SHOW_TRIGGER = Animator.StringToHash("Show");

        [SerializeField] protected Animator animator;

        [Space]
        [SerializeField] protected Transform visualsParent;
        [SerializeField] protected Transform defaultTargetRingVisuals;

        public EnemyBehavior Target { get; protected set; }
        protected EnemyBehavior nextTarget;
        protected bool isHiding = false;

        protected Dictionary<GameObject, Transform> visuals = new Dictionary<GameObject, Transform>();

        protected virtual void Awake()
        {
            transform.SetParent(null);
        }

        protected virtual void Start()
        {
            StageController.Player.onClosestEnemyChanged += SetTarget;
        }

        public virtual void SetTarget(EnemyBehavior target)
        {
            if (Target == target || nextTarget == target && target != null)
                return;

            if (Target == null)
            {
                if (target == null) return;

                Target = target;

                Show();
            }
            else
            {
                if (!isHiding)
                {
                    isHiding = true;
                    animator.SetTrigger(HIDE_TRIGGER);
                }

                nextTarget = target;
            }
        }

        protected virtual void Update()
        {
            if (Target != null)
            {
                transform.position = Target.Position;
            }
        }

        protected virtual void OnTargetRingShowAnimationEnded()
        {

        }

        protected virtual void OnTargetRingHideAnimationEnded()
        {
            isHiding = false;

            Target = nextTarget;

            Show();
        }

        protected virtual void Show()
        {
            if (Target != null)
            {
                animator.SetTrigger(SHOW_TRIGGER);

                foreach (var visual in visuals.Values)
                {
                    visual.gameObject.SetActive(false);
                }

                if (Target.TargetRingPrefab == null)
                {
                    defaultTargetRingVisuals.gameObject.SetActive(true);
                }
                else
                {
                    defaultTargetRingVisuals.gameObject.SetActive(false);

                    if (!visuals.TryGetValue(Target.TargetRingPrefab, out Transform targetVisual))
                    {
                        targetVisual = Instantiate(Target.TargetRingPrefab, visualsParent).transform;
                        visuals.Add(Target.TargetRingPrefab, targetVisual);
                    }

                    targetVisual.gameObject.SetActive(true);
                }
            }
        }

        protected void OnDestroy()
        {
            StageController.Player.onClosestEnemyChanged -= SetTarget;
        }
    }
}