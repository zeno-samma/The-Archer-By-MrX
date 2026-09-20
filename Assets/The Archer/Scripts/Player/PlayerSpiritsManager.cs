using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class PlayerSpiritsManager : MonoBehaviour
    {
        [SerializeField] protected List<Transform> spiritsPositions;

        protected List<SpiritBehavior> spirits = new List<SpiritBehavior>();

        public MultiplicativeStat SpiritsDamageStat { get; protected set; }
        public MultiplicativeStat SpiritAttackDelayStat { get; protected set; }

        protected virtual void Awake()
        {
            SpiritsDamageStat = 1;
            SpiritAttackDelayStat = 1;
        }

        public virtual void RegisterSpirit(SpiritBehavior spiritBehavior)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            spirits.Add(spiritBehavior);

            Transform spiritPosition;
            if (spiritsPositions.Count < spirits.Count)
            {
                spiritPosition = spiritsPositions[^1];
            }
            else
            {
                spiritPosition = spiritsPositions[spirits.Count - 1];
            }
            spiritBehavior.transform.position = spiritPosition.position;
            spiritBehavior.SetPosition(spiritPosition);

            spiritBehavior.DamageStat.AddChildStat(SpiritsDamageStat);
            spiritBehavior.AttackDelayStat.AddChildStat(SpiritAttackDelayStat);
        }

        public virtual void RemoveSpirit(SpiritBehavior spiritBehavior)
        {
            var index = spirits.IndexOf(spiritBehavior);

            spirits.RemoveAt(index);

            for (int i = index; i < spirits.Count; i++)
            {
                var position = spiritsPositions.Count > i ? spiritsPositions[i] : spiritsPositions[^1];
                spirits[i].SetPosition(position);
            }

            spiritBehavior.DamageStat = null;
            spiritBehavior.AttackDelayStat = null;
        }

        public virtual void ResetPosition()
        {
            for (int i = 0; i < spirits.Count; i++)
            {
                spirits[i].HardResetPosition();
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (spiritsPositions == null) return;

            foreach (var position in spiritsPositions)
            {
                if (position != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(position.position, 0.25f);
                }

            }
        }
    }
}

