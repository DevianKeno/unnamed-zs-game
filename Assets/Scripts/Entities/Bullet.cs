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
        [SerializeField] EntityData entityData;
        public override EntityData EntityData => entityData;
        public BulletEntityOptions BulletEntityOptions;

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

        public override void OnSpawn()
        {
        }

        public void SetTrajectoryFromPlayer(Player player)
        {
            transform.SetPositionAndRotation(
                player.EyeLevel,
                Quaternion.LookRotation(player.Forward)
            );
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

        public void OnCollisionEnter(Collision collision)
        {
            throw new NotImplementedException();
        }
    }
}