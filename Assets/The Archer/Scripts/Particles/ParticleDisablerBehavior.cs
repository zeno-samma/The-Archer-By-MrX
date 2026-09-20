using UnityEngine;

namespace OctoberStudio.Particles
{
    public class ParticleDisablerBehavior : MonoBehaviour
    {
        [SerializeField] protected float particleLifetime;

        protected float disableTime;

        protected void OnEnable()
        {
            disableTime = Time.time + particleLifetime;
        }

        protected void Update()
        {
            if(Time.time >= disableTime)
            {
                gameObject.SetActive(false);
            }
        }
    }
}