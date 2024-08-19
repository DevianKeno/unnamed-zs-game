using System;
using UnityEngine;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{        
    public class PlayerDetection: MonoBehaviour 
    {
        public event EventHandler<EnemyCollidedEventArgs> OnEnemyInRange;
        public float SiteRange, AttackRange, CritterSiteRange; // range from which Players is in site/attack
        Enemy _enemyFound; // the enemy found in a specific collider
        Wildlife _critterFound; // the critter found in a specific collider
        Player _player;
        Collider[] _hitSiteColliders, _hitAttackColliders, _hitSiteWildlifeColliders; // array of object that is within entity range
        [SerializeField] LayerMask EnemyLayer, WildlifeLayer; // Layers that the enemy chases

        void FixedUpdate()
        {
            FindEnemyInRange();
            FindCritterInRange();
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
            Gizmos.DrawWireSphere(transform.position, CritterSiteRange); // Draw the wireframe sphere
        }

        public void FindEnemyInRange()
        {
            // Find enemy within the range
            _hitSiteColliders = Physics.OverlapSphere(transform.position, SiteRange, EnemyLayer);
            _hitAttackColliders = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayer);

            // Find any player component
            _player = GetComponent<Player>();

            // Iterate over each enemy in site
            foreach (var collider in _hitSiteColliders)
            {
                // If detected enemy chase player
                if (collider.TryGetComponent<Enemy>(out _enemyFound))
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
                // If detected enemy chase player
                if (collider.TryGetComponent<Enemy>(out _enemyFound))
                {
                    if (collider.TryGetComponent<IDetectable>(out var detectable))
                    {
                        // Player is within enemy site range
                        detectable.PlayerAttackDetect(_player);
                    }
                }
            }
        }

        public void FindCritterInRange()
        {
            // Find Wildlife in range
            _hitSiteWildlifeColliders = Physics.OverlapSphere(transform.position, CritterSiteRange, WildlifeLayer);

            // Find any player component
            _player = GetComponent<Player>();

            // Iterate over each enemy in site
            foreach (var collider in _hitSiteWildlifeColliders)
            {
                _critterFound = collider.GetComponent<Wildlife>();
                // If detected enemy chase player
                if (_critterFound != null && collider.TryGetComponent<IDetectable>(out var detectable))
                {
                    // Player is within enemy site range
                    detectable.PlayerSiteDetect(_player);
                }
            }
        }
    }
}