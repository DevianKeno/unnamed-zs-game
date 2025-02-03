using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.UI;

namespace UZSG.Players
{
    public class PlayerDetection : MonoBehaviour 
    {
        [SerializeField] Player player;

        [SerializeField] internal float _healthBarScanRadius = 10f;

        public float PlayerDetectionRange;
        public float PlayerAttackableRange;
        public LayerMask Layers;

        List<IAttributable> _trackedHealthBars = new();
        
        void FixedUpdate()
        {
            CastDetectionSphere();
        }

        Collider[] _detectedCollidersResults = new Collider[10];
        Collider[] _attackableCollidersResults = new Collider[10];
        public void CastDetectionSphere()
        {
            int dCount = Physics.OverlapSphereNonAlloc(transform.position, PlayerDetectionRange, _detectedCollidersResults, Layers);
            int aCount = Physics.OverlapSphereNonAlloc(transform.position, PlayerAttackableRange, _attackableCollidersResults, Layers);
            
            for (int i = 0; i < dCount; i++)
            {
                var collider = _detectedCollidersResults[i];
                if (!collider.TryGetComponent<IPlayerDetectable>(out var detected)) continue;

                var ettyDistance = Vector3.Distance(detected.Position, player.Position);
                if (ettyDistance <= detected.PlayerDetectionRadius) /// inverse detection, respect skill values
                {
                    detected.DetectPlayer(player);
                }
                
                if (detected is IHasHealthBar ihhb)
                {
                    if (ettyDistance <= _healthBarScanRadius)
                        // IsVisible(player, detected))
                    {
                        ihhb.HealthBar.Player = player;
                        ihhb.HealthBar.Show();
                    }
                    else
                    {
                        ihhb.HealthBar.Hide();
                    }
                }
            }

            for (int i = 0; i < aCount; i++)
            {
                var collider = _attackableCollidersResults[i];

                if (!collider.TryGetComponent<IPlayerDetectable>(out var detectable)) return;
                
                if (Vector3.Distance(detectable.Position, player.Position) <= detectable.PlayerAttackableRadius)
                {
                    detectable.AttackPlayer(player);
                }
            }
        }

        /// <summary>
        /// Check whether the detectable is visible from the Player's camera perspective.
        /// </summary>
        bool IsVisible(Player player, IPlayerDetectable detectable)
        {
            Vector3 direction = (detectable.Position - player.MainCamera.transform.position).normalized;
            float distance = Vector3.Distance(player.Position, detectable.Position);

            if (Physics.Raycast(player.EyeLevel, direction, out var hit, distance))
            {
                if (hit.collider.TryGetComponent<IPlayerDetectable>(out var detected) &&
                    detected == detectable) /// check if same entity
                {
                    return true;
                }
            }

            return false;
        }
                
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 20);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 2);
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