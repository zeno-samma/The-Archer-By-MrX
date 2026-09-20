using System.Collections.Generic;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class AdditiveStat
    {
        protected int initialValue;
        public int Value { get; protected set; }

        protected List<StatAdder> adders = new List<StatAdder>();
        protected List<AdditiveStat> childrenStats = new List<AdditiveStat>();

        public UnityAction<AdditiveStat> onStatChanged;

        public static implicit operator int(AdditiveStat stat) => stat.Value;
        public static implicit operator AdditiveStat(int value) => new AdditiveStat(value);

        public AdditiveStat(int initialValue)
        {
            this.initialValue = initialValue;
            Value = initialValue;
        }

        public virtual void AddChildStat(AdditiveStat child)
        {
            if (!childrenStats.Contains(child))
            {
                childrenStats.Add(child);
                RecalculateValue();
                child.onStatChanged += OnChildStatChanged;
            }
        }

        public virtual void RemoveChildStat(AdditiveStat child)
        {
            if (childrenStats.Remove(child))
            {
                RecalculateValue();
                child.onStatChanged -= OnChildStatChanged;
            }
        }

        protected virtual void OnChildStatChanged(AdditiveStat childStat)
        {
            RecalculateValue();
        }

        public virtual void AddAdder(StatAdder adder)
        {
            adders.Add(adder);
            RecalculateValue();
            adder.onAdderChanged += OnStatAdderChanged;
        }

        public virtual void RemoveAdder(StatAdder adder)
        {
            if (adders.Remove(adder))
            {
                RecalculateValue();
                adder.onAdderChanged -= OnStatAdderChanged;
            }
        }

        protected virtual void OnStatAdderChanged(StatAdder adder)
        {
            RecalculateValue();
        }

        protected virtual void RecalculateValue()
        {
            var prevValue = Value;

            Value = initialValue;

            foreach (var adder in adders)
                Value += adder;

            foreach (var child in childrenStats)
                Value += child;

            if (prevValue != Value)
                onStatChanged?.Invoke(this);
        }
    }
}