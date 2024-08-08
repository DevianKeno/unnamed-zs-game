using System;
using UnityEngine;
using UZSG.Entities;
using UZSG.Interactions;

namespace UZSG.Players
{        
    public class PlayerDetection: MonoBehaviour 
    {
        public event EventHandler<EnemyCollidedEventArgs> OnEnemyInRange;
        public float SiteRange, AttackRange; // range from which Players is in site, attack range of entity
        public float _shortestDistance = Mathf.Infinity;
        bool _inChase;
        Player _player;
        Collider[] _hitColliders; // array of object that is within enemy range
        Collider _closestCollider;
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

        public void FindEnemyInRange()
        {
            // Reset the shortest distance and closest collider
            _shortestDistance = float.MaxValue;
            _closestCollider = null;
            
            // Find objects within the range
            _hitColliders = Physics.OverlapSphere(transform.position, SiteRange, EnemyLayer);
            _player = GetComponent<Player>();

            foreach (var collider in _hitColliders)
            {
                if (collider.TryGetComponent<IDetectable>(out var detectable))
                {
                    detectable.PlayerDetect(_player);
                }
            }
        }
    }
}