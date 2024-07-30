using System;
using UnityEngine;
using UZSG.Items.Weapons;
using UZSG.Systems;

namespace UZSG.Entities
{
    public struct BulletEntityOptions
    {
        public Vector3 Origin { get; set; }
        /// <summary>
        /// Distance where the bullet starts to be visible.
        /// </summary>
        public float RenderAt { get; set; }
        public float Damage { get; set; }
        public Vector3 Velocity { get; set; }
        public float Speed { get; set; }
    }

    public interface ICollision
    {
        public string CollisionTag { get; }
    }
    
    /// <summary>
    /// Bullet entity.
    /// This bullet entity is a suitable candidate for practicing Entity Component System.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Bullet : Entity, IProjectile, ICollision
    {
        public const float DefaultBulletScale = 0.05f;
        
        public string CollisionTag => "Projectile";
        public BulletDamageAttributes DamageAttributes;
        public BulletAttributes Attributes;
        public float CalculatedDamage;
        
        public Vector3 Point;
        Vector3 _velocity;
        Vector3 _origin;
        Vector3 _previousPosition;
        bool _checkDistance;
        bool _minRenderDistanceNormalized;
        bool _isMoving;
        bool _hasGravity;

        Rigidbody rb;
        BoxCollider coll;
        MeshRenderer meshRenderer;
        TrailRenderer trailRenderer;
        
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            coll = GetComponent<BoxCollider>();
            meshRenderer = GetComponentInChildren<MeshRenderer>();
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        void Start()
        {
            Destroy(gameObject, 5f); /// Despawn after 5 seconds
        }
        
        void Update()
        {
            ApplyMovement();
            ApplyGravity();
            RaycastForCollisions();
            _previousPosition = transform.position;
        }

        void FixedUpdate()
        {
            CheckDistanceFromOrigin();
        }

        void RaycastForCollisions()
        {
            Vector3 direction = rb.velocity.normalized;
            float distance = (transform.position - _previousPosition).magnitude;
            Ray ray = new(_previousPosition, direction);
            
            if (Physics.Raycast(ray, out var hit, distance, LayerMask.GetMask("Hitbox")))
            {
                OnHit(hit.point, hit.collider);
            }
        }

        void ApplyMovement()
        {
            if (!_isMoving) return;
            
            Vector3 target = _previousPosition + _velocity * (Attributes.Speed * Time.deltaTime);
            
            if (_previousPosition != target)
            {
                Vector3 displacement = target - _previousPosition;
                rb.velocity = displacement / Game.Tick.SecondsPerTick;
            }
        }

        void ApplyGravity()
        {
            if (!_hasGravity) return;

            rb.AddForce(Physics.gravity * Attributes.GravityScale);
        }

        void CheckDistanceFromOrigin()
        {
            if (!_checkDistance) return;

            /// TODO: Render bullet at the start pa lang if in bullet time
            if (Vector3.Distance(_origin, transform.position) > Attributes.MinRenderDistance)
            {
                _checkDistance = false;
                meshRenderer.enabled = true;
                trailRenderer.enabled = true;
            }
        }

        void OnHit(Vector3 point, Collider other)
        {
            if (other.TryGetComponent<Hitbox>(out var hitbox))
            {
                CalculatedDamage = CalculateDamage(hitbox.Part);
                hitbox.HitBy(new()
                {
                    By = this,
                    ContactPoint = point,
                });
                Destroy(gameObject);
            }
        }

        float CalculateDamage(HitboxPart part)
        {
            float damage = DamageAttributes.BaseDamage;

            if (DamageAttributes.IsPartDamage)
            {
                damage = part switch
                {
                    HitboxPart.Head => DamageAttributes.HeadDamage,
                    HitboxPart.Body => DamageAttributes.BodyDamage,
                    HitboxPart.Arms => DamageAttributes.ArmsDamage,
                    HitboxPart.Legs => DamageAttributes.LegsDamage,
                    _ => DamageAttributes.BaseDamage
                };
            }

            if (DamageAttributes.UseMultiplier)
            {
                float multiplier = part switch
                {
                    HitboxPart.Head => DamageAttributes.HeadDamageMultiplier,
                    HitboxPart.Body => DamageAttributes.BodyDamageMultiplier,
                    HitboxPart.Arms => DamageAttributes.ArmsDamageMultiplier,
                    HitboxPart.Legs => DamageAttributes.LegsDamageMultiplier,
                    _ => 1f
                };

                damage *= multiplier;
            }

            return damage;
        }

        public void SetTrajectory(Vector3 direction)
        {
            _velocity = direction;
        }

        public void SetPlayerAndShoot(Player player)
        {
            _origin = transform.position;
            _previousPosition = transform.position;
            _checkDistance = Attributes.MinRenderDistance > 0;
            
            transform.SetPositionAndRotation(
                player.EyeLevel,
                Quaternion.LookRotation(_velocity)
            );
            
            _isMoving = true;
        }
 
        public void SetBulletAttributes(BulletAttributes options)
        {
            Attributes = options;
            meshRenderer.transform.localScale = new(
                DefaultBulletScale * Attributes.Scale,
                DefaultBulletScale * Attributes.Scale,
                DefaultBulletScale * Attributes.Scale
            );
            if (Attributes.GravityScale != 0)
            {
                _hasGravity = true;
                // rb.useGravity = true;
                // rb.gravity = attributes.GravityScale;
            }
            if (Attributes.MinRenderDistance > 0)
            {
                meshRenderer.enabled = false;
                trailRenderer.enabled = false;
            }
        }
    }
}