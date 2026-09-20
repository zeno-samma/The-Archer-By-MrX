using OctoberStudio.Drop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class RageStarAbilityBehavior : AbilityBehavior<RageStarAbilityData, RageStarAbilityLevel>
    {
        [SerializeField] protected GameObject rageParticleObject;

        protected List<DropBehavior> spawnedDrop = new List<DropBehavior>();

        protected bool isAbilityActive = false;
        protected float abilityStartTime = 0f;

        protected StatMultiplier attackSpeedMultiplier = 1;
        protected StatAdder critChangeAdder = 0;

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            StageController.Player.Stats.AttackSpeedStat.AddMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.CritChanceStat.AddAdder(critChangeAdder);

            StageController.DoAfterRoomLoaded(() =>
            {
                StartCoroutine(AbilityCoroutine());
            });
        }

        protected override void SetAbilityLevel(int levelId)
        {
            base.SetAbilityLevel(levelId);

            if(isAbilityActive)
            {
                attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
                critChangeAdder.Value = AbilityLevel.CritChangeIncreasePercent;
            }
        }

        protected virtual void Update()
        {
            if (isAbilityActive && abilityStartTime + AbilityLevel.AbilityDuration <= Time.time)
            {
                isAbilityActive = false;
                attackSpeedMultiplier.Value = 1f;
                critChangeAdder.Value = 0;

                rageParticleObject.SetActive(false);
            }
        }

        protected virtual IEnumerator AbilityCoroutine()
        {
            yield return null;

            while (true)
            {
                if (StageController.Room.AliveEnemiesCount > 0)
                {
                    var randomPosition = StageController.NavigationManager.GetRandomPosition();

                    var drop = StageController.DropManager.Drop(DropType.RageStar, randomPosition, 0);
                    drop.onPickedUp += OnDropPickedUp;

                    spawnedDrop.Add(drop);
                }

                yield return new WaitForSeconds(AbilityLevel.StarSpawnInterval * StageController.Player.Stats.StarSpawnIntervalMultiplierStat);
            }
        }

        protected virtual void OnDropPickedUp(DropBehavior drop)
        {
            drop.onPickedUp -= OnDropPickedUp;
            spawnedDrop.Remove(drop);

            abilityStartTime = Time.time;

            if (!isAbilityActive)
            {
                isAbilityActive = true;
                attackSpeedMultiplier.Value = AbilityLevel.AttackSpeedMultiplier;
                critChangeAdder.Value = AbilityLevel.CritChangeIncreasePercent;

                rageParticleObject.SetActive(true);
            }
        }

        public override void Clear()
        {
            StageController.Player.Stats.AttackSpeedStat.RemoveMultiplier(attackSpeedMultiplier);
            StageController.Player.Stats.CritChanceStat.RemoveAdder(critChangeAdder);

            for(int i = 0; i < spawnedDrop.Count; i++)
            {
                var drop = spawnedDrop[i];
                drop.onPickedUp -= OnDropPickedUp;

                StageController.DropManager.HideDrop(drop);
            }

            spawnedDrop.Clear();

            base.Clear();
        }
    }
}