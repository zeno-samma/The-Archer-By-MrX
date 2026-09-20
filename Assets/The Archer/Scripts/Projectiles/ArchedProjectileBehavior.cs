using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Projectile
{
    public class ArchedProjectileBehavior : SimpleProjectile
    {
        [SerializeField] protected float maxHeight = 4f;
        [SerializeField] protected AnimationCurve trajectoryCurve;
        [SerializeField] protected float minDuration = 1f;
        [SerializeField] protected float maxDuration = 5f;

        [SerializeField] protected List<TrailRenderer> trailsToResetOnDisable;

        public Vector3 TargetPosition { get; set; }
        protected Vector3 startPosition;

        public override void Launch(float damage)
        {
            base.Launch(damage);

            startPosition = transform.position;

            if (Target != null) TargetPosition = Target.position;

            var distance = Vector3.Distance(startPosition, TargetPosition);
            var duration = distance / Speed;

            if (duration < minDuration) Speed = distance / minDuration;
            if (duration > maxDuration) Speed = distance / maxDuration;

            foreach (var trail in trailsToResetOnDisable)
            {
                trail.gameObject.SetActive(true);
                if (trail != null) trail.Clear();
            }
        }

        protected override void Update()
        {
            if (Target != null && Target.gameObject.activeSelf)
            {
                TargetPosition = Vector3.Lerp(TargetPosition, Target.position, magnetism);
            }

            var direction = (TargetPosition - transform.position).SetY(0).normalized;

            var overalDistance = (startPosition - TargetPosition).SetY(0).magnitude;
            var traveledDistance = (transform.position - startPosition).SetY(0).magnitude;

            var t = traveledDistance / overalDistance;

            if (t >= 1)
            {
                Hide();
                return;
            }

            var y = trajectoryCurve.Evaluate(t) * maxHeight;
            var position = transform.position + direction * Time.deltaTime * Speed;

            position.y = y;

            transform.position = position;
        }

        public override void Hide()
        {
            base.Hide();

            foreach (var trail in trailsToResetOnDisable)
            {
                trail.gameObject.SetActive(false);
                if (trail != null) trail.Clear();
            }
        }
    }
}