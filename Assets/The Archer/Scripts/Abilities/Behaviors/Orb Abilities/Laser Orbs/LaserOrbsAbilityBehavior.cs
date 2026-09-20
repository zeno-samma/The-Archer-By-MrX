using OctoberStudio.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class LaserOrbsAbilityBehavior : AbilityBehavior<LaserOrbsAbilityData, LaserOrbsAbilityLevel>, IOrbsAbility
    {
        [SerializeField] protected Transform axisTransform;

        [SerializeField] protected List<OrbProjectileBehavior> orbs;
        [SerializeField] protected List<LaserProjectileBehavior> lasers;

        protected float globalDamageMultiplier = 1;

        protected WaitForSeconds disabledWait;
        protected WaitForSeconds activeWait;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.OrbsManager.RegisterOrbs(this);

            StartCoroutine(LasersCoroutine());
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            foreach (var orb in orbs)
            {
                orb.Launch(StageController.Player.Damage * AbilityLevel.OrbDamageMultiplier * globalDamageMultiplier);
            }

            foreach (var laser in lasers)
            {
                laser.Launch(StageController.Player.Damage * AbilityLevel.LaserDamageMultiplier * globalDamageMultiplier);
            }

            disabledWait = new WaitForSeconds(AbilityLevel.LaserDisabledDuration);
            activeWait = new WaitForSeconds(AbilityLevel.LaserActiveDuration);
        }

        protected virtual IEnumerator LasersCoroutine()
        {
            foreach (var laser in lasers)
            {
                laser.Hide();
            }

            
            while (true)
            {
                yield return disabledWait;

                if (StageController.Room.AliveEnemiesCount > 0)
                {
                    foreach (var laser in lasers)
                    {
                        laser.gameObject.SetActive(true);
                    }

                    yield return activeWait;

                    foreach (var laser in lasers)
                    {
                        laser.Hide();
                    }
                }
            }
        }

        public virtual void SetGlobalOrbsDamageMultiplier(float damageMultiplier)
        {
            globalDamageMultiplier = damageMultiplier;

            foreach (var orb in orbs)
            {
                orb.Launch(StageController.Player.Damage * AbilityLevel.OrbDamageMultiplier * globalDamageMultiplier);
            }

            foreach (var laser in lasers)
            {
                laser.Launch(StageController.Player.Damage * AbilityLevel.LaserDamageMultiplier * globalDamageMultiplier);
            }
        }

        public virtual void SetAngle(float angle)
        {
            axisTransform.eulerAngles = Vector3.up * angle;
        }

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public virtual void ResetTrails()
        {
            for (int i = 0; i < orbs.Count; i++)
            {
                orbs[i].ClearTrails();
            }
        }

        public override void Clear()
        {
            StageController.Player.OrbsManager.RemoveOrbs(this);

            base.Clear();
        }
    }
}