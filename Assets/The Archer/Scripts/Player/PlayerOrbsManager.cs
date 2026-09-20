using OctoberStudio.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class PlayerOrbsManager : MonoBehaviour
    {
        [SerializeField] protected float orbsAngularSpeed = 180;

        protected List<IOrbsAbility> orbsAbilities = new List<IOrbsAbility>();
        protected List<IOrbsAbility> soloOrbsAbilities = new List<IOrbsAbility>();

        protected float angle = 0f;

        public virtual void RegisterOrbs(IOrbsAbility orbsAbility, bool soloOrbs = false)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            if (soloOrbs)
            {
                soloOrbsAbilities.Add(orbsAbility);
            }
            else
            {
                orbsAbilities.Add(orbsAbility);
            }
        }

        public virtual void RemoveOrbs(IOrbsAbility orbsAbility)
        {
            if (orbsAbilities.Contains(orbsAbility))
            {
                orbsAbilities.Remove(orbsAbility);
            }

            if (soloOrbsAbilities.Contains(orbsAbility))
            {
                soloOrbsAbilities.Remove(orbsAbility);
            }
        }

        public virtual void SetOrbsDamageMultiplier(float damageMultiplier)
        {
            foreach (var ability in orbsAbilities)
            {
                ability.SetGlobalOrbsDamageMultiplier(damageMultiplier);
            }

            foreach (var ability in soloOrbsAbilities)
            {
                ability.SetGlobalOrbsDamageMultiplier(damageMultiplier);
            }
        }

        public virtual void ResetTrails()
        {
            for (int i = 0; i < orbsAbilities.Count; i++)
            {
                orbsAbilities[i].ResetTrails();
            }

            for (int i = 0; i < soloOrbsAbilities.Count; i++)
            {
                soloOrbsAbilities[i].ResetTrails();
            }
        }

        protected virtual void Update()
        {
            angle += Time.deltaTime * orbsAngularSpeed;
            if (angle > 360) angle -= 360f;

            UpdateOrbsList(orbsAbilities);
            UpdateOrbsList(soloOrbsAbilities);
        }

        protected virtual void UpdateOrbsList(List<IOrbsAbility> orbsList)
        {
            if (orbsList.Count > 0)
            {
                float step = 180f / orbsList.Count;

                for (int i = 0; i < orbsList.Count; i++)
                {
                    orbsList[i].SetAngle(angle + step * i);
                    orbsList[i].SetPosition(transform.position);
                }
            }
        }

        public virtual void Clear()
        {
            orbsAbilities.Clear();
            soloOrbsAbilities.Clear();
        }
    }
}