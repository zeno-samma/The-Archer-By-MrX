using System.Collections;
using MrX.Projectile;
using UnityEngine;
using UnityEngine.AI;

namespace MrX.Enemy
{
    public enum SkeletonBossRank { Minion, Warrior, Rogue, Mage }

    /// <summary>Animation-event driven attacks; all markers are authored children of the boss prefab.</summary>
    public class SkeletonBossBehavior : EnemyBehavior
    {
        [Header("Skeleton Boss")]
        [SerializeField] private SkeletonBossRank rank;
        [SerializeField, Min(.2f)] private float recovery = 1.6f;
        [SerializeField, Min(.3f)] private float warningDuration = .9f;
        [SerializeField, Min(.5f)] private float attackRange = 3.4f;
        [SerializeField, Min(.5f)] private float groundRadius = 2.8f;
        [SerializeField, Range(10,180)] private float cleaveAngle = 100;
        [SerializeField, Min(0)] private float specialDamageMultiplier = 1.5f;
        [SerializeField, Min(1)] private float chargeDistance = 8;
        [SerializeField, Min(.1f)] private float chargeDuration = .7f;
        [SerializeField, Min(.2f)] private float chargeHalfWidth = .9f;
        [SerializeField, Range(3,24)] private int projectileCount = 5;
        [SerializeField, Range(10,120)] private float fanAngle = 55;
        [Header("Ranged cadence and gap closer")]
        [SerializeField, Min(1)] private int shotsBeforeSkill = 4;
        [SerializeField, Min(.05f)] private float shotRecovery = .18f;
        [SerializeField, Min(.05f)] private float shotWarning = .12f;
        [SerializeField, Min(.1f)] private float attackPlaybackSpeed = 2.5f;
        [SerializeField, Min(1)] private float gapCloseMinDistance = 4;
        [SerializeField, Min(.2f)] private float gapCloseCooldown = 3.5f;
        [SerializeField, Min(.2f)] private float gapCloseWarning = .5f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileOrigin;
        [SerializeField] private Transform circleMarker;
        [SerializeField] private Transform coneMarker;
        [SerializeField] private Transform lineMarker;
        [SerializeField] private Transform[] runeMarkers;
        private readonly Vector3[] runePositions = new Vector3[3];
        private Vector3 aim, attackOrigin, chargeEnd;
        private int cycle;
        private bool pendingImpact, impacted, attackEnded;
        private bool special;
        private bool gapClosing, runeSkill;
        private int shotsSinceSkill, skillCycle;
        private float nextGapClose;
        public SkeletonBossRank Rank => rank;
        public int CompletedAttacks { get; private set; }
        public int NormalShots { get; private set; }
        public int SkillsUsed { get; private set; }
        public int GapClosersUsed { get; private set; }

        protected override void Start()
        {
            base.Start();
            if (projectilePrefab != null) StageController.ProjectilesManager.RegisterProjectile(projectilePrefab);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            cycle=0; CompletedAttacks=0; pendingImpact=impacted=attackEnded=false;
            NormalShots=SkillsUsed=GapClosersUsed=shotsSinceSkill=skillCycle=0;
            nextGapClose=0;gapClosing=runeSkill=false;
            LookAtPlayer=false; PoisonDamageMultiplier=1;
            HideMarkers();
            if(animator!=null){animator.Rebind();animator.speed=1;}
        }
        protected override void OnDisable()
        {
            StopAllCoroutines();
            pendingImpact=false; LookAtPlayer=false; HideMarkers();
            base.OnDisable();
        }
        private IEnumerator WaitAction(float seconds)
        {
            float elapsed=0;
            while(elapsed<seconds && IsAlive){elapsed+=Time.deltaTime*Mathf.Clamp01(animator.speed);yield return null;}
        }
        protected override IEnumerator BehaviorCoroutine()
        {
            yield return WaitAction(1.1f);
            while(IsAlive)
            {
                if(!TryFindTarget() || Target==null || !Target.IsAlive){yield return null;continue;}
                LookAtPlayer=true;
                bool ranged=rank==SkeletonBossRank.Rogue || rank==SkeletonBossRank.Mage;
                float walk=0;
                float approachRange=ranged?attackRange:Time.time>=nextGapClose?Mathf.Max(attackRange,chargeDistance):attackRange;
                while(IsAlive && Target.IsAlive && HorizontalDistance(Position,Target.Position)>approachRange && walk<3)
                {
                    TryMove(Target.Position);walk+=Time.deltaTime*Mathf.Clamp01(animator.speed);yield return null;
                }
                navigationHandler.Stop();
                yield return WaitAction(ranged?0:.1f);
                if(!Target.IsAlive)continue;
                LookAtPlayer=false;
                attackOrigin=Position;aim=Target.Position-Position;aim.y=0;
                if(aim.sqrMagnitude<.01f)aim=transform.forward;aim.Normalize();
                transform.rotation=Quaternion.LookRotation(aim);
                gapClosing=!ranged && Time.time>=nextGapClose && HorizontalDistance(Position,Target.Position)>=gapCloseMinDistance;
                if(ranged)
                {
                    special=shotsSinceSkill>=Mathf.Max(1,shotsBeforeSkill);
                    runeSkill=rank==SkeletonBossRank.Mage && special && skillCycle%2==0;
                }
                else special=!gapClosing && (cycle++%2)==1;
                animator.SetFloat("Attack Cadence",ranged||gapClosing?attackPlaybackSpeed:1.2f);
                PrepareMarkers();
                yield return WaitAction(gapClosing?gapCloseWarning:ranged&&!special?shotWarning:warningDuration);
                if(!IsAlive || !Target.IsAlive){HideMarkers();continue;}
                pendingImpact=true;impacted=false;attackEnded=false;
                animator.SetInteger(ATTACK_TYPE_INT,special?1:0);
                animator.SetTrigger(ATTACK_TRIGGER);
                // Bounded wait protects a malformed animation without applying untelegraphed fallback damage.
                float timeout=0;
                while(!impacted && IsAlive && timeout<5){timeout+=Time.deltaTime*Mathf.Clamp01(animator.speed);yield return null;}
                if(impacted && gapClosing)
                {
                    yield return Charge();
                    nextGapClose=Time.time+gapCloseCooldown;
                }
                timeout=0;
                while(!attackEnded && IsAlive && timeout<3){timeout+=Time.deltaTime*Mathf.Clamp01(animator.speed);yield return null;}
                pendingImpact=false;HideMarkers();
                if(impacted && ranged)
                {
                    if(special){shotsSinceSkill=0;skillCycle++;}else shotsSinceSkill++;
                }
                if(rank==SkeletonBossRank.Rogue && special && IsAlive)yield return Sidestep();
                yield return WaitAction(gapClosing?.25f:ranged&&!special?shotRecovery:recovery);
            }
        }
        private void Marker(Transform marker,Vector3 point,Vector3 scale,Quaternion rotation)
        {
            if(marker==null)return;marker.position=point+Vector3.up*.06f;marker.rotation=rotation;marker.localScale=scale;marker.gameObject.SetActive(true);
        }
        private void PrepareMarkers()
        {
            HideMarkers();
            if(gapClosing)
            {
                float distance=Mathf.Clamp(HorizontalDistance(Position,Target.Position)-.8f,0,chargeDistance);
                chargeEnd=Position+aim*distance;
                NavMeshHit hit;
                if(NavMesh.Raycast(Position,chargeEnd,out hit,NavMesh.AllAreas))chargeEnd=hit.position-aim*.5f;
                if(NavMesh.SamplePosition(chargeEnd,out hit,1,NavMesh.AllAreas))chargeEnd=hit.position;else chargeEnd=Position;
                Marker(lineMarker,(Position+chargeEnd)*.5f,new Vector3(chargeHalfWidth*2,.02f,HorizontalDistance(Position,chargeEnd)),Quaternion.LookRotation(aim));
            }
            else if(rank==SkeletonBossRank.Minion)
            {
                if(special)Marker(circleMarker,Position,Vector3.one*groundRadius,Quaternion.identity);
                else Marker(coneMarker,Position,Vector3.one*attackRange,Quaternion.LookRotation(aim));
            }
            else if(rank==SkeletonBossRank.Warrior)
            {
                if(special)Marker(circleMarker,Position,Vector3.one*groundRadius,Quaternion.identity);
                else Marker(coneMarker,Position,Vector3.one*attackRange,Quaternion.LookRotation(aim));
            }
            else if(rank==SkeletonBossRank.Rogue || !special)
            {
                if(special)Marker(coneMarker,Position,Vector3.one*attackRange,Quaternion.LookRotation(aim));
            }
            else if(runeSkill)
            {
                for(int i=0;i<3;i++)
                {
                    var offset=Quaternion.Euler(0,i*120,0)*Vector3.forward*(i==0?0:groundRadius*2.6f);
                    runePositions[i]=Target.Position+offset;runePositions[i].y=Position.y;
                    NavMeshHit hit;if(NavMesh.SamplePosition(runePositions[i],out hit,2,NavMesh.AllAreas))runePositions[i]=hit.position;
                    if(i<runeMarkers.Length)Marker(runeMarkers[i],runePositions[i],Vector3.one*groundRadius,Quaternion.identity);
                }
            }
            else Marker(circleMarker,Position,Vector3.one*groundRadius,Quaternion.identity);
        }
        public override void OnAttackEventFired()
        {
            if(!pendingImpact || impacted || !IsAlive || Time.timeScale<=0 || Target==null || !Target.IsAlive)return;
            impacted=true;pendingImpact=false;CompletedAttacks++;
            if(gapClosing){GapClosersUsed++;return;}
            if(special)SkillsUsed++;
            if(rank==SkeletonBossRank.Minion)
            {
                if(special)CircleDamage(attackOrigin,groundRadius);
                else
                {
                    CleaveDamage();
                }
            }
            else if(rank==SkeletonBossRank.Warrior){if(special){CircleDamage(attackOrigin,groundRadius);Volley(12,360);}else CleaveDamage();}
            else if(rank==SkeletonBossRank.Rogue){Volley(special?projectileCount+2:1,special?fanAngle:0);if(!special)NormalShots++;}
            else if(!special){Volley(1,0);NormalShots++;}
            else if(runeSkill){bool inside=false;for(int i=0;i<3;i++)if(HorizontalDistance(Target.Position,runePositions[i])<=groundRadius)inside=true;if(inside)HitTarget();}
            else Volley(projectileCount,360);
            HideMarkers();
        }
        private void HitTarget()
        {
            if(Target==null || !Target.IsAlive)return;
            // Ground and cleave attacks cannot pass through the arena's solid walls.
            if(Physics.Linecast(Position+Vector3.up*.5f,Target.Position+Vector3.up*.5f,1<<LayerMask.NameToLayer("Obstacle")))return;
            Target.TakeDamage(Damage*(special?specialDamageMultiplier:1),DamageType.Physical);
        }
        private static float HorizontalDistance(Vector3 a,Vector3 b){a.y=b.y=0;return Vector3.Distance(a,b);}
        private void CleaveDamage(){var delta=Target.Position-attackOrigin;delta.y=0;if(delta.magnitude<=attackRange && Vector3.Angle(aim,delta)<=cleaveAngle*.5f)HitTarget();}
        private void CircleDamage(Vector3 point,float radius){if(Target!=null&&HorizontalDistance(Target.Position,point)<=radius)HitTarget();}
        private void Volley(int count,float spread)
        {
            if(projectilePrefab==null)return;
            count=Mathf.Clamp(count,1,24);
            for(int i=0;i<count;i++)
            {
                float angle=spread>=360?i*360f/count:(count==1?0:-spread*.5f+i*spread/(count-1));
                var projectile=StageController.ProjectilesManager.GetProjectile(projectilePrefab);
                var simple=projectile as SimpleProjectile;if(simple!=null)simple.ResetOverrides();
                projectile.transform.position=projectileOrigin!=null?projectileOrigin.position:Position+Vector3.up;
                projectile.transform.rotation=Quaternion.LookRotation(Quaternion.Euler(0,angle,0)*aim);
                projectile.transform.localScale=Vector3.one;
                projectile.Target=null;projectile.IsPiercingEnabled=false;projectile.IgnoresObstacles=false;
                // Reach the engagement boundary even for slower magic bolts; preserve readable speed.
                if(simple!=null && (rank==SkeletonBossRank.Rogue || rank==SkeletonBossRank.Mage))
                    projectile.Lifetime=Mathf.Max(projectile.Lifetime,(attackRange+2)/Mathf.Max(.1f,simple.Speed));
                projectile.Launch(Damage*(special?specialDamageMultiplier:1));
            }
        }
        private IEnumerator Charge()
        {
            float elapsed=0;var start=Position;bool hit=false;
            while(elapsed<chargeDuration && IsAlive)
            {
                if(Time.timeScale<=0 || animator.speed<=0){yield return null;continue;}
                var old=Position;elapsed+=Time.deltaTime*Mathf.Clamp01(animator.speed);
                var next=Vector3.Lerp(start,chargeEnd,Mathf.Clamp01(elapsed/Mathf.Max(.1f,chargeDuration)));
                NavMeshHit nav;if(NavMesh.SamplePosition(next,out nav,.6f,NavMesh.AllAreas))navigationHandler.Teleport(nav.position);
                if(lineMarker!=null)lineMarker.position=(start+chargeEnd)*.5f+Vector3.up*.06f;
                if(!hit && Target!=null && Target.IsAlive)
                {
                    var segment=Position-old;var rel=Target.Position-old;segment.y=rel.y=0;
                    var t=segment.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(rel,segment)/segment.sqrMagnitude):0;
                    if(HorizontalDistance(Target.Position,old+segment*t)<=chargeHalfWidth){HitTarget();hit=true;}
                }
                yield return null;
            }
            HideMarkers();
        }
        private IEnumerator Sidestep()
        {
            var point=Position+Vector3.Cross(Vector3.up,aim)*(cycle%2==0?3:-3);
            if(TryMove(point)){float t=0;while(t<.55f && IsAlive){t+=Time.deltaTime*Mathf.Clamp01(animator.speed);yield return null;}navigationHandler.Stop();}
        }
        public override void OnAttackEndedEventFired(){attackEnded=true;}
        protected override void OnTriggerEnter(Collider other)
        {
            // The swept charge already applies one hit; contact must not apply a second hit.
            if(gapClosing || !IsAlive || Time.timeScale<=0)return;
            base.OnTriggerEnter(other);
        }
        public override void OnSpawnEndedEventFired(){waitForSpawnToEnd.Complete();}
        protected override void Defeat(){pendingImpact=false;HideMarkers();base.Defeat();}
        private void HideMarkers()
        {
            if(circleMarker!=null)circleMarker.gameObject.SetActive(false);
            if(coneMarker!=null)coneMarker.gameObject.SetActive(false);
            if(lineMarker!=null)lineMarker.gameObject.SetActive(false);
            if(runeMarkers!=null)foreach(var marker in runeMarkers)if(marker!=null)marker.gameObject.SetActive(false);
        }
    }
}
