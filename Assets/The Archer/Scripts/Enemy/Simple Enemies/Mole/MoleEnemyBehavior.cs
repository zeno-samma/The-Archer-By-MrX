using OctoberStudio.Audio;
using System.Collections;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class MoleEnemyBehavior : EnemyBehavior
    {
        [Header("Timings")]
        [SerializeField] protected Float idleDuration = (2, 3);

        [Header("Attack")]
        [SerializeField] protected Float attackDuration = (3f, 4f);
        [SerializeField] protected Float minimumAttackDuration = 0.3f;
        [SerializeField] protected Float distanceToPlayerToEmerge = 0.8f;
        [SerializeField] protected Float emergeColliderRadius = (1);

        [Header("Particles")]
        [SerializeField] protected ParticleSystem burrowInParticle;
        [SerializeField] protected ParticleSystem burrowOutParticle;
        [SerializeField] protected ParticleSystem burrowedParticle;

        [Header("Sounds")]
        [SerializeField] protected AudioData burrowInSound;
        [SerializeField] protected AudioData burrowOutSound;
        [SerializeField] protected AudioData burrowedSound;

        protected CapsuleCollider moleCollider;

        protected Coroutine moveToPlayerCoroutine;

        protected AudioSource burrowedAudioSource;

        protected override void Awake()
        {
            base.Awake();

            moleCollider = enemyCollider as CapsuleCollider;
        }

        protected override IEnumerator BehaviorCoroutine()
        {
            yield return null;

            while (IsAlive)
            {
                LookAtPlayer = true;

                yield return new WaitForSeconds(idleDuration);

                LookAtPlayer = false;

                yield return AttackCoroutine();
            }
        }

        protected virtual IEnumerator AttackCoroutine()
        {
            animator.SetTrigger(ATTACK_TRIGGER);

            if (burrowInParticle != null) burrowInParticle.Play();
            GameController.AudioManager.PlayAudio(burrowInSound);

            yield return new WaitForSeconds(minimumAttackDuration);

            var endTime = Time.time + attackDuration - minimumAttackDuration;
            waitForAttackToEnd.Reset();
            yield return new WaitUntil(() => Time.time > endTime || !waitForAttackToEnd);

            animator.SetTrigger(ATTACK_ENDED_TRIGGER);

            if(moveToPlayerCoroutine != null) StopCoroutine(moveToPlayerCoroutine);
            navigationHandler.Stop();

            enemyCollider.enabled = true;

            var cahceRadius = moleCollider.radius;
            moleCollider.radius = emergeColliderRadius;

            if (burrowOutParticle != null) burrowOutParticle.Play();
            if (burrowedParticle != null) burrowedParticle.Stop();

            if (burrowedAudioSource != null)
            {
                burrowedAudioSource.loop = false;
                burrowedAudioSource.Stop();
                burrowedAudioSource = null;
            }
            GameController.AudioManager.PlayAudio(burrowInSound);

            healthbar.Show();

            yield return new WaitForSeconds(0.1f);

            moleCollider.radius = cahceRadius;
        }

        public override void OnAttackEventFired()
        {
            base.OnAttackEventFired();

            enemyCollider.enabled = false;

            healthbar.Hide();

            if(burrowedParticle != null) burrowedParticle.Play();
            burrowedAudioSource = GameController.AudioManager.PlayAudio(burrowedSound);
            if (burrowedAudioSource != null) burrowedAudioSource.loop = true;

            moveToPlayerCoroutine = StartCoroutine(MoveToPlayerCoroutine());
        }

        protected virtual IEnumerator MoveToPlayerCoroutine()
        {
            while (IsAlive)
            {
                TryFindTarget();

                var targetPosition = transform.position;
                
                if(Target != null)
                {
                    targetPosition = Target.Position;

                    if(Vector3.Distance(transform.position, targetPosition) < distanceToPlayerToEmerge)
                    {
                        waitForAttackToEnd.Complete();
                        yield break;
                    }
                }

                navigationHandler.Move(targetPosition);
                yield return new WaitForSeconds(0.25f);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StageController.onDefeat += OnPlayerDefeated;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            StageController.onDefeat -= OnPlayerDefeated;
        }

        protected virtual void OnPlayerDefeated()
        {
            if (burrowedAudioSource != null)
            {
                burrowedAudioSource.loop = false;
                burrowedAudioSource.Stop();
                burrowedAudioSource = null;
            }
        }

        protected override void Defeat()
        {
            base.Defeat();

            StopAllCoroutines();

            if (burrowedAudioSource != null)
            {
                burrowedAudioSource.loop = false;
                burrowedAudioSource.Stop();
                burrowedAudioSource = null;
            }
        }
    }
}