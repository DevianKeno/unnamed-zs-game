using System;
using UnityEngine;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{        
    public class PlayerDetection: MonoBehaviour 
    {
        public event EventHandler<EnemyCollidedEventArgs> OnEnemyInRange;
        public float SiteRange, AttackRange; // range from which Players is in site/attack
        Enemy _enemyFound; // the enemy found in a specific collider
        Player _player;
        Collider[] _hitSiteColliders, _hitAttackColliders; // array of object that is within enemy range
        [SerializeField] LayerMask EnemyLayer; // Layers that the enemy chases

        void FixedUpdate()
        {
            FindEnemyInRange();
        }

        public struct EnemyCollidedEventArgs
        {   
            public Player Player;
            public bool InChase;
            public Collider[] Enemies;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red; // Set the color of the gizmo
            Gizmos.DrawWireSphere(transform.position, SiteRange); // Draw the wireframe sphere
        }

        public void FindEnemyInRange()
        {
            // Find objects within the range
            _hitSiteColliders = Physics.OverlapSphere(transform.position, SiteRange, EnemyLayer);
            _hitAttackColliders = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayer);

            // Find any player component
            _player = GetComponent<Player>();

            // Iterate over each enemy in site
            foreach (var collider in _hitSiteColliders)
            {
                _enemyFound = collider.GetComponent<Enemy>();
                // If detected enemy chase player
                if (_enemyFound != null)
                {
                    if (collider.TryGetComponent<IDetectable>(out var detectable))
                    {
                        // Player is within enemy site range
                        detectable.PlayerSiteDetect(_player);
                    }
                }
            }

            // Iterate over each enemy in attack
            foreach (var collider in _hitAttackColliders)
            {
                _enemyFound = collider.GetComponent<Enemy>();
                // If detected enemy chase player
                if (_enemyFound != null)
                {
                    if (collider.TryGetComponent<IDetectable>(out var detectable))
                    {
                        // Player is within enemy site range
                        detectable.PlayerAttackDetect(_player);
                    }
                }
            }
        }
    }
}