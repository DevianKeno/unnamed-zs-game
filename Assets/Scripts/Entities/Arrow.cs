using System;

using UnityEngine;

using UZSG.Interactions;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    // /// <summary>
    // /// Bullet entity.
    // /// This bullet entity is a suitable candidate for practicing Entity Component System.
    // /// </summary>
    // [RequireComponent(typeof(Rigidbody))]
    // [RequireComponent(typeof(BoxCollider))]
    // public class Arrow : Entity, IProjectile, ICollisionSource
    // {        
    //     public string CollisionTag => "Projectile";
    //     public LayerMask LayerMask;
        
    //     Vector3 _velocity;
    //     Vector3 _origin;
    //     Vector3 _previousPosition;
    //     bool _checkDistance;
    //     bool _minRenderDistanceNormalized;
    //     bool _isMoving;
    //     bool _hasGravity;

    //     Rigidbody rb;
    //     BoxCollider coll;
    //     MeshRenderer meshRenderer;
    //     TrailRenderer trailRenderer;
        
    //     void Awake()
    //     {
    //         rb = GetComponent<Rigidbody>();
    //         coll = GetComponent<BoxCollider>();
    //         meshRenderer = GetComponentInChildren<MeshRenderer>();
    //         trailRenderer = GetComponentInChildren<TrailRenderer>();
    //     }

    //     void Start()
    //     {
    //         Destroy(gameObject, 5f); /// Despawn after 5 seconds
    //     }
        
    //     void Update()
    //     {
    //     }

    //     void FixedUpdate()
    //     {
    //         ApplyMovement();
    //         ApplyGravity();
    //         RaycastForCollisions();
    //         CheckDistanceFromOrigin();
    //         _previousPosition = transform.position;
    //     }

    //     void RaycastForCollisions()
    //     {
    //         Vector3 direction = rb.velocity.normalized;
    //         float distance = (transform.position - _previousPosition).magnitude;
    //         Ray ray = new(_previousPosition, direction);
            
    //         if (Physics.Raycast(ray, out var hit, distance, LayerMask))
    //         {
    //             OnHit(hit.point, hit.collider);
    //         }
    //     }

    //     void ApplyMovement()
    //     {
    //         if (!_isMoving) return;
            
    //         Vector3 target = _previousPosition + _velocity * (Attributes.Speed * Time.deltaTime);
            
    //         if (_previousPosition != target)
    //         {
    //             Vector3 displacement = target - _previousPosition;
    //             rb.velocity = displacement / Game.Tick.SecondsPerTick;
    //         }
    //     }

    //     void ApplyGravity()
    //     {
    //         if (!_hasGravity) return;

    //         rb.AddForce(Physics.gravity * Attributes.GravityScale);
    //     }

    //     // void CheckDistanceFromOrigin()
    //     // {
    //     //     if (!_checkDistance) return;

    //     //     /// TODO: Render bullet at the start pa lang if in bullet time
    //     //     if (Vector3.Distance(_origin, transform.position) > Attributes.MinRenderDistance)
    //     //     {
    //     //         _checkDistance = false;
    //     //         meshRenderer.enabled = true;
    //     //         trailRenderer.enabled = true;
    //     //     }
    //     // }

    //     void OnHit(Vector3 point, Collider hitObject)
    //     {
    //         var info = new CollisionHitInfo()
    //         {
    //             Type = CollisionType.Melee,
    //             Source = this,
    //             ContactPoint = point,
    //         };

    //         var target = hitObject.GetComponentInParent<ICollisionTarget>();
    //         if (target != null)
    //         {
    //             // CalculatedDamage = CalculateDamage(hitbox.Part);
    //             info.Target = target;
    //             target.HitBy(info);
    //             Destroy(gameObject);
    //         }
    //     }

    //     float CalculateDamage(HitboxPart part)
    //     {
    //         float damage = DamageAttributes.BaseDamage;

    //         if (DamageAttributes.IsPartDamage)
    //         {
    //             damage = part switch
    //             {
    //                 HitboxPart.Head => DamageAttributes.HeadDamage,
    //                 HitboxPart.Body => DamageAttributes.BodyDamage,
    //                 HitboxPart.Arms => DamageAttributes.ArmsDamage,
    //                 HitboxPart.Legs => DamageAttributes.LegsDamage,
    //                 _ => DamageAttributes.BaseDamage
    //             };
    //         }

    //         if (DamageAttributes.UseMultiplier)
    //         {
    //             float multiplier = part switch
    //             {
    //                 HitboxPart.Head => DamageAttributes.HeadDamageMultiplier,
    //                 HitboxPart.Body => DamageAttributes.BodyDamageMultiplier,
    //                 HitboxPart.Arms => DamageAttributes.ArmsDamageMultiplier,
    //                 HitboxPart.Legs => DamageAttributes.LegsDamageMultiplier,
    //                 _ => 1f
    //             };

    //             damage *= multiplier;
    //         }

    //         return damage;
    //     }

    //     public void SetTrajectory(Vector3 direction)
    //     {
    //         _velocity = direction;
    //     }

    //     public void SetPlayerAndShoot(Player player)
    //     {
    //         _origin = player.Position;
    //         _previousPosition = _origin;
    //         _checkDistance = Attributes.MinRenderDistance > 0;
            
    //         transform.SetPositionAndRotation(
    //             player.EyeLevel,
    //             Quaternion.LookRotation(_velocity)
    //         );
            
    //         _isMoving = true;
    //     }
    // }
}