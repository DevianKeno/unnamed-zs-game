using System;
using UnityEngine;
using UnityEngine.UIElements;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{
    public class PlayerDetection : MonoBehaviour 
    {
        [SerializeField] Player player;
        public LayerMask Layers;
        public float PlayerDetectionRange, PlayerAttackableRange;

        void FixedUpdate()
        {
            CastDetectionSphere();
            // FindEnemyInRange();
            // FindCritterInRange();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 20);
        }

        public void CastDetectionSphere()
        {
            Collider[] detectedColliders = Physics.OverlapSphere(transform.position, PlayerDetectionRange, Layers);
            foreach (var collider in detectedColliders)
            {
                if (collider.TryGetComponent<IPlayerDetectable>(out var detectable))
                {
                    if (Vector3.Distance(detectable.Position, player.Position) <= detectable.PlayerDetectionRadius)
                    {
                        detectable.DetectPlayer(player);
                    }
                }
            }
        }
        /*public LayerMask 
            EnemyLayer,
            WildlifeLayer; // Layers that the enemy chases

        public float SiteRange, AttackRange, CritterSiteRange; // range from which Players is in site/attack

        /// <summary>
        /// the enemy found in a specific collider
        /// </summary>
        Enemy _enemyFound; 
        /// <summary>
        /// the critter found in a specific collider
        /// </summary>
        Wildlife _detectedWildlife; 
        /// <summary>
        /// the critter found in a specific collider
        /// </summary>
        IPlayerDetectable _detectedEntity;

        /// Array of objects that is within entity range
        Collider[] _hitSiteColliders,
            _hitAttackColliders,
            _hitSiteWildlifeColliders;*/

        /*public void FindEnemyInRange()
        {
            // Find enemy within the range
            _hitSiteColliders = Physics.OverlapSphere(transform.position, SiteRange, EnemyLayer);
            _hitAttackColliders = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayer);

            // Iterate over each enemy in site
            foreach (var collider in _hitSiteColliders)
            {
                // If detected enemy chase player
                if (collider.TryGetComponent<Enemy>(out _enemyFound))
                {
                    if (collider.TryGetComponent<IPlayerDetectable>(out var detectable))
                    {
                        // Player is within enemy site range
                        detectable.DetectPlayer(player);
                    }
                }
            }

            // Iterate over each enemy in attack
            foreach (var collider in _hitAttackColliders)
            {
                // If detected enemy chase player
                if (collider.TryGetComponent<Enemy>(out _enemyFound))
                {
                    if (collider.TryGetComponent<IPlayerDetectable>(out var detectable))
                    {
                        // Player is within enemy site range
                        detectable.PlayerAttackDetect(player);
                    }
                }
            }
        }

        public void FindCritterInRange()
        {
            // Find Wildlife in range
            _hitSiteWildlifeColliders = Physics.OverlapSphere(transform.position, CritterSiteRange, WildlifeLayer);

            // Iterate over each enemy in site
            foreach (var collider in _hitSiteWildlifeColliders)
            {
                _detectedWildlife = collider.GetComponent<Wildlife>();
                // If detected enemy chase player
                if (_detectedWildlife != null && collider.TryGetComponent<IPlayerDetectable>(out var detectable))
                {
                    // Player is within enemy site range
                    detectable.DetectPlayer(player);
                }
            }
        }*/
    }
}