using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Interactions;

namespace UZSG.Entities
{
    public class Skinwalker : Enemy
    {
        EntityHitboxController hitboxes;

        void Awake()
        {
            hitboxes = GetComponent<EntityHitboxController>();
        }

        protected override void Start()
        {
            base.Start();
            OnSpawn();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            InitializeHitboxEvents();
        }

        void InitializeHitboxEvents()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                hitbox.OnHit += OnCollision;
            }
        }

        void OnCollision(object sender, CollisionHitInfo info)
        {
            if (info.Source.CollisionTag == "Projectile")
            {
                var hitbox = sender as Hitbox;
                var bullet = info.Source as Bullet;

                TakeDamage(10f);

                Debug.Log($"Shot part {hitbox.Part}");
                SpawnBlood(info.ContactPoint);
                SpawnDamageText(info.ContactPoint);
                Destroy(bullet.gameObject); /// Change on penetration
            }
            else if (info.Source.CollisionTag == "Melee")
            {
                SpawnBlood(info.ContactPoint);
                SpawnDamageText(info.ContactPoint);
            }
        }

        void TakeDamage(float damage)
        {
            var health = Attributes.Get("health");
            print(health.Value);
            health.Remove(damage);
            print(health.Value);
        }

        void SpawnDamageText(Vector3 position)
        {
            Game.Entity.Spawn<DamageText>("damage_text", position);
        }

        void SpawnBlood(Vector3 position)
        {
            Game.Particles.Create("blood_splat", position);
        }
    }
}