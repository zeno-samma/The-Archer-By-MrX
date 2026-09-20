using System.Collections.Generic;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class MultiplicativeStat
    {
        protected float initialValue;
        public float InitialValue => initialValue;
        public float Value { get; protected set; }

        protected List<StatMultiplier> multipliers = new List<StatMultiplier>();
        protected List<MultiplicativeStat> childrenStats = new List<MultiplicativeStat>();

        public UnityAction<MultiplicativeStat> onStatChanged;

        public static implicit operator float(MultiplicativeStat multiplier) => multiplier.Value;
        public static implicit operator MultiplicativeStat(float value) => new MultiplicativeStat(value);

        public MultiplicativeStat(float initialValue)
        {
            this.initialValue = initialValue;

            Value = initialValue;
        }

        public virtual bool IsModified()
        {
            return initialValue != Value;
        }

        public virtual void ChangeInitialValue(float newInitialValue)
        {
            initialValue = newInitialValue;
            RecalculateValue();
        }

        public virtual void AddChildStat(MultiplicativeStat child)
        {
            if (!childrenStats.Contains(child))
            {
                childrenStats.Add(child);

                RecalculateValue();

                child.onStatChanged += OnChildStatChanged;
            }
        }

        public virtual void RemoveChildStat(MultiplicativeStat child)
        {
            childrenStats.Remove(child);

            RecalculateValue();

            child.onStatChanged -= OnChildStatChanged;
        }

        protected virtual void OnChildStatChanged(MultiplicativeStat childStat)
        {
            RecalculateValue();
        }

        public virtual void AddMultiplier(StatMultiplier multiplier)
        {
            if (multipliers.Contains(multiplier)) return;

            multipliers.Add(multiplier);

            RecalculateValue();

            multiplier.onMultiplierChanged += OnStatMultiplierChanged;
        }

        public virtual void RemoveMultiplier(StatMultiplier multiplier)
        {
            multipliers.Remove(multiplier);

            RecalculateValue();

            multiplier.onMultiplierChanged -= OnStatMultiplierChanged;
        }

        protected virtual void OnStatMultiplierChanged(StatMultiplier multiplier)
        {
            RecalculateValue();
        }

        protected virtual void RecalculateValue()
        {
            var prevValue = Value;

            Value = initialValue;

            for (int i = 0; i < multipliers.Count; i++)
            {
                Value *= multipliers[i];
            }

            for (int i = 0; i < childrenStats.Count; i++)
            {
                Value *= childrenStats[i];
            }

            if (prevValue != Value)
            {
                onStatChanged?.Invoke(this);
            }
        }
    }
}