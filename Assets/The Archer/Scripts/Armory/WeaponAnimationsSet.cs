using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Armory
{
    [CreateAssetMenu(fileName = "Weapon Animations Set", menuName = "October/Armory/Weapon Animations Set")]
    public class WeaponAnimationsSet : ScriptableObject
    {
        [Tooltip("This value is used to calculate the value of movement speed multipler property of hero animator")]
        [SerializeField] protected float movementAnimationSpeed = 5;
        [SerializeField] protected WeaponAnimationsSetType weaponAnimationsSetType;
        [SerializeField] protected List<WeaponAnimation> animations;
        [SerializeField] protected RuntimeAnimatorController runtimeAnimatorController;
        [SerializeField] protected float attackAnimationLength;

        public WeaponAnimationsSetType WeaponAnimationsSetType => weaponAnimationsSetType;

        public int AnimationsCount => animations.Count;
        public List<WeaponAnimation> Animations => animations;

        public RuntimeAnimatorController RuntimeAnimatorController => runtimeAnimatorController;

        public float MovementAnimationSpeed => movementAnimationSpeed;
        public float AttackAnimationLength => attackAnimationLength;

        public virtual WeaponAnimation GetAnimation(string name)
        {
            return animations.Find(animation => animation.Name == name);
        }
    }

    [System.Serializable]
    public class WeaponAnimation
    {
        [SerializeField] protected string name;
        [SerializeField] protected AnimationClip animationClip;

        public string Name => name;
        public AnimationClip AnimationClip => animationClip;
    }

    public enum WeaponAnimationsSetType
    {
        Separate_Animations = 0,
        Animator = 1
    }
}