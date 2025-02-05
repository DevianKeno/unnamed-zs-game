using System.Collections.Generic;

using UnityEngine;

using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Systems;
using UZSG.UI;

namespace UZSG.Players
{
    public class PlayerDetection : MonoBehaviour 
    {
        public Player Player { get; private set; }

        [SerializeField] float detectionRange; /// set in Inspector
        [SerializeField] float attackableRange; /// set in Inspector /// TODO: subject to be moved to another script
        /// <summary>
        /// Minimum distance to display health bars.
        /// </summary>
        [SerializeField] float healthBarScanRadius; /// set in Inspector
        [SerializeField] LayerMask Layers; /// set in Inspector
        
        void Awake()
        {
            Player = GetComponentInParent<Player>();
        }

        void FixedUpdate()
        {
            CastDetectionSphere();
        }

        Collider[] _detectedCollidersResults = new Collider[10];
        // Collider[] _attackableCollidersResults = new Collider[10];
        public void CastDetectionSphere()
        {
            int dCount = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, _detectedCollidersResults, Layers);
            for (int i = 0; i < dCount; i++)
            {
                var collider = _detectedCollidersResults[i];
                if (!collider.TryGetComponent<Entity>(out var etty)) continue;

                if (etty is IPlayerDetectable detectable)
                {
                    if (!detectable.IsAlive) continue;

                    detectable.NotifyDetection(Player);/// inverse detection, should respect skill values
                    
                    if (etty is IHasHealthBar ihhb)
                    {
                        if (this.Player.InRangeOf(etty.Position, healthBarScanRadius))// &&
                            // this.Player.CanSee(etty))
                        {
                            ihhb.HealthBar.Player = Player;
                            ihhb.HealthBar.Show();
                        }
                        else
                        {
                            ihhb.HealthBar.Hide();
                        }
                    }
                }
            }

            // int aCount = Physics.OverlapSphereNonAlloc(transform.position, PlayerAttackableRange, _attackableCollidersResults, Layers);
            // for (int i = 0; i < aCount; i++)
            // {
            //     var collider = _attackableCollidersResults[i];

            //     if (!collider.TryGetComponent<IPlayerDetectable>(out var detectable)) return;
                
            //     if (Vector3.Distance(detectable.Position, player.Position) <= detectable.PlayerAttackableRadius)
            //     {
            //         detectable.AttackPlayer(player);
            //     }
            // }
        }
        
        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(transform.position, 20);

        //     Gizmos.color = Color.green;
        //     Gizmos.DrawWireSphere(transform.position, 2);
        // }

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