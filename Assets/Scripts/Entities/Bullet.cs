using System;

using UnityEngine;

using UZSG.Interactions;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    public struct BulletEntityOptions
    {
        /// <summary>
        /// Position where the bullet originated.
        /// </summary>
        public Vector3 Origin { get; set; }
        /// <summary>
        /// Distance where the bullet starts to be visible.
        /// </summary>
        public float RenderAt { get; set; }
        /// <summary>
        /// Bullet's internal damage.
        /// </summary>
        public float Damage { get; set; }
        /// <summary>
        /// Bullet trajectory.
        /// </summary>
        public Vector3 Velocity { get; set; }
        public float Speed { get; set; }
    }
    
    /// <summary>
    /// Bullet entity.
    /// This bullet entity is a suitable candidate for practicing Entity Component System.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Bullet : Entity, IProjectile, ICollisionSource, IDamageSource
    {
        public const float DefaultBulletScale = 0.08f;
        public const float LifetimeSeconds = 3f;
        
        public string CollisionTag => "Projectile";
        public BulletDamageAttributes DamageAttributes;
        public BulletAttributes BulletAttributes;
        public LayerMask LayerMask;

        bool _isMoving;
        bool _hasGravity;
        bool _checkDistance;
        Vector3 _velocity;
        Vector3 _origin;
        Vector3 _previousPosition;

        [SerializeField] Rigidbody rb;
        [SerializeField] BoxCollider coll;
        [SerializeField] MeshRenderer meshRenderer;
        [SerializeField] TrailRenderer trailRenderer;
        
        public override void OnSpawnEvent()
        {
            base.OnSpawnEvent();

            Destroy(gameObject, LifetimeSeconds); /// Despawn after 5 seconds
        }

        void FixedUpdate()
        {
            RaycastForCollisions();
            ApplyMovement();
            ApplyGravity();
            CheckDistanceFromOrigin();
            _previousPosition = transform.position;
        }

        void RaycastForCollisions()
        {
            Vector3 direction = rb.velocity.normalized;
            float distance = (transform.position - _previousPosition).magnitude;
            Ray ray = new(_previousPosition, direction);
            
            if (Physics.Raycast(ray, out var hit, distance, LayerMask))
            {
                OnHit(hit.point, hit.collider);
            }
        }

        void ApplyMovement()
        {
            if (!_isMoving) return;
            
            Vector3 target = _previousPosition + _velocity * (BulletAttributes.Speed * Time.deltaTime);
            
            if (_previousPosition != target)
            {
                Vector3 displacement = target - _previousPosition;
                rb.velocity = displacement / Game.Tick.SecondsPerTick;
            }
        }

        void ApplyGravity()
        {
            if (!_hasGravity) return;

            rb.AddForce(Physics.gravity * BulletAttributes.GravityScale);
        }

        /// <summary>
        /// Check the distance of this bullet from where it spawned and render the mesh upon reaching the threshold.
        /// </summary>
        void CheckDistanceFromOrigin()
        {
            if (!_checkDistance) return;

            /// TODO: Render bullet at the start pa lang if in bullet time
            if (Vector3.Distance(_origin, transform.position) > BulletAttributes.MinRenderDistance)
            {
                _checkDistance = false;
                meshRenderer.enabled = true;
                trailRenderer.enabled = true;
            }
        }

        void OnHit(Vector3 point, Collider hitObject)
        {
            var info = new HitboxCollisionInfo()
            {
                ObjectType = ObjectCollisionType.Projectile,
                Source = this,
                ContactPoint = point,
            };

            var target = hitObject.GetComponentInParent<ICollisionTarget>();
            if (target != null)
            {
                // CalculatedDamage = CalculateDamage(hitbox.Part);
                info.Target = target;
                target.HitBy(info);
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

        /// <summary>
        /// Make this bullet be owned by the given Player and make it fly.
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayerAndShoot(Player player)
        {
            _origin = player.EyeLevel;
            _previousPosition = _origin;
            _checkDistance = BulletAttributes.MinRenderDistance > 0;
            
            transform.SetPositionAndRotation(
                player.EyeLevel,
                Quaternion.LookRotation(_velocity)
            );
            
            _isMoving = true;
        }
 
        public void SetBulletAttributes(BulletAttributes options)
        {
            BulletAttributes = options;
            meshRenderer.transform.localScale = new(
                DefaultBulletScale * BulletAttributes.Scale,
                DefaultBulletScale * BulletAttributes.Scale,
                DefaultBulletScale * BulletAttributes.Scale
            );
            if (BulletAttributes.GravityScale != 0)
            {
                _hasGravity = true;
                // rb.useGravity = true;
                // rb.gravity = attributes.GravityScale;
            }
            if (BulletAttributes.MinRenderDistance > 0)
            {
                meshRenderer.enabled = false;
                trailRenderer.enabled = false;
            }
        }
    }
}