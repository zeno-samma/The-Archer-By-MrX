using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio.Weapon
{
    public class FlyingWeaponHolder : MonoBehaviour
    {
        [SerializeField] protected Transform weaponParent;
        [SerializeField] protected AnimationCurve flyingWeaponSpeedDistanceCurve;
        [SerializeField] protected ParticleSystem particle;

        protected Transform parent;
        protected Vector3 offset;

        protected AbstractWeaponBehavior weapon;

        protected Rigidbody weaponParentRigidbody;

        public bool IsDetached { get; protected set; }

        protected virtual void Awake()
        {
            offset = transform.localPosition;
            parent = transform.parent;

            weaponParentRigidbody = weaponParent.GetComponent<Rigidbody>();
        }

        public virtual void ResetPosition()
        {
            transform.position = parent.position + offset;
        }

        public virtual void Detach(AbstractWeaponBehavior weapon)
        {
            this.weapon = weapon;

            IsDetached = true;

            transform.SetParent(null);
            weapon.transform.SetParent(weaponParent);
            
            weaponParent.localPosition = weapon.DetachedPosition;
            weaponParent.localEulerAngles = weapon.DetachedRotation;
            weaponParent.localScale = weapon.DetachedScale;

            weapon.transform.ResetLocal();

            if (particle != null) particle.Play();
        }

        public virtual void Attach()
        {
            transform.SetParent(parent);
            transform.ResetLocal();

            transform.localPosition = offset;

            IsDetached = false;

            if (particle != null) particle.Stop();
        }

        protected virtual void Update()
        {
            if (!IsDetached) return;

            var target = StageController.Player.ClosestEnemy;
            if (target != null)
            {
                var rotationToTarget = Quaternion.LookRotation(transform.position.DirectionToXZ(target.Position), Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, Time.deltaTime * 8);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, Time.deltaTime * 8);
            }

            var desiredPosition = parent.position + offset;

            var heroPosition = StageController.Player.Position;
            var heroDirection = heroPosition.DirectionToXZ(desiredPosition);

            var ray = new Ray(heroPosition, heroDirection);

            if (Physics.Raycast(ray, out var hit, Vector3.Distance(heroPosition, desiredPosition.SetY(heroPosition.y)), LayerMask.GetMask("Obstacle")))
            {
                desiredPosition = hit.point.SetY(desiredPosition.y) + hit.normal.SetY(0).normalized * 0.3f;
            }

            var distance = (transform.position - desiredPosition).SetY(0).magnitude;
            if (distance > 0.1f)
            {
                var direction = transform.position.DirectionToXZ(desiredPosition);

                var speed = flyingWeaponSpeedDistanceCurve.Evaluate(distance);

                var frameMove = direction * Time.deltaTime * speed;
                if (frameMove.magnitude >= distance)
                {
                    transform.position = desiredPosition;
                }
                else
                {
                    transform.position += frameMove;
                }
            }

            weaponParent.localPosition = weapon.DetachedPosition;
        }
    }
}