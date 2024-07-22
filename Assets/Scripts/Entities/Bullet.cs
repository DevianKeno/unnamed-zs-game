using System;
using UnityEngine;

namespace UZSG.Entities
{
    public struct BulletEntityOptions
    {
        public float Damage { get; set; }
        public Vector3 Velocity { get; set; }
        public float Speed { get; set; }
    }

    /// <summary>
    /// Bullet entity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Bullet : Entity
    {
        public BulletEntityOptions BulletEntityOptions;

        Vector3 _previousPosition;

        Rigidbody rb;
        BoxCollider coll;
        
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            coll = GetComponent<BoxCollider>();
        }

        void Start()
        {
            /// Despawn after 5 seconds
            Destroy(gameObject, 5f); 
        }
        
        void Update()
        {
            RaycastForCollisions();
            _previousPosition = transform.position;
        }

        void RaycastForCollisions()
        {
            Vector3 direction = rb.velocity.normalized;
            float distance = (transform.position - _previousPosition).magnitude;
            Ray ray = new(_previousPosition, direction);
            if (Physics.Raycast(ray, out var hit, distance, LayerMask.GetMask("Hitbox")))
            {
                OnHit(hit.collider);
            }
        }

        void OnHit(Collider other)
        {
            if (other.TryGetComponent<Hitbox>(out var hitbox))
            {
                hitbox.Collision(coll);
                Destroy(gameObject);
            }
        }

        public override void OnSpawn()
        {
        }

        public void SetTrajectoryFromPlayer(Player player)
        {
            transform.SetPositionAndRotation(
                player.EyeLevel,
                Quaternion.LookRotation(player.Forward)
            );
            _previousPosition = transform.position;
        }

        public void SetBulletEntityOptions(BulletEntityOptions options)
        {
            BulletEntityOptions = options;
        }

        public void Shoot()
        {
            var attr = BulletEntityOptions;
            rb.AddForce(attr.Velocity * attr.Speed, ForceMode.Impulse);
        }
    }
}