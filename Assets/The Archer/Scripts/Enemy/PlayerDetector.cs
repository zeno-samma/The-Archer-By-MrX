using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class PlayerDetector : MonoBehaviour
    {
        protected List<PlayerBehavior> detectedPlayers = new List<PlayerBehavior>();

        public bool HasDetectedPlayers => detectedPlayers.Count > 0;

        private void Awake()
        {
            var enemy = GetComponentInParent<EnemyBehavior>();
            enemy.RegisterPlayerDetector(this);
        }

        public PlayerBehavior GetClosestPlayer()
        {
            var closestDistance = float.MaxValue;
            PlayerBehavior closestPlayer = null;

            foreach (var player in detectedPlayers)
            {
                var distance = (transform.position - player.Position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }

        public List<PlayerBehavior> GetDetectedPlayers()
        {
            return new List<PlayerBehavior>(detectedPlayers);
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerBehavior>();
            if (player != null && !detectedPlayers.Contains(player))
            {
                detectedPlayers.Add(player);
                player.SubscribeOnDefeat(OnPlayerDefeated);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponent<PlayerBehavior>();
            if (player != null)
            {
                detectedPlayers.Remove(player);
                player.UnsubscribeOnDefeat(OnPlayerDefeated);
            }
        }

        protected virtual void OnPlayerDefeated(IDefeatable defeatable)
        {
            var player = (PlayerBehavior)defeatable;
            if (player != null)
            {
                detectedPlayers.Remove(player);
                player.UnsubscribeOnDefeat(OnPlayerDefeated);
            }
        }
    }
}