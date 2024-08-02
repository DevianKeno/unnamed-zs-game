using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class Skinwalker : Enemy
    {
        EntityHitboxController hitboxes;

        void Awake()
        {
            hitboxes = GetComponent<EntityHitboxController>();
        }

        void Start()
        {
            InitializeHitboxEvents();
        }

        public override void OnSpawn()
        {
            InitializeHitboxEvents();
        }

        void InitializeHitboxEvents()
        {
            foreach (var hitbox in hitboxes.Hitboxes)
            {
                hitbox.OnCollision += OnCollision;
            }
        }

        void OnCollision(object sender, CollisionHitInfo info)
        {
            if (info.By.CollisionTag == "Projectile")
            {
                var hitbox = sender as Hitbox;
                var bullet = info.By as Bullet;

                Debug.Log($"Shot part {hitbox.Part}");
                SpawnBlood(info.ContactPoint);
                // SpawnDamageText(info.ContactPoint);
                Destroy(bullet.gameObject);
            }
            else if (info.By.CollisionTag == "Melee")
            {
                SpawnBlood(info.ContactPoint);
                // SpawnDamageText(info.ContactPoint);
            }
        }

        void SpawnDamageText(Vector3 position)
        {
            Game.Entity.Spawn<DamageText>("damage_text", position);
        }

        void SpawnBlood(Vector3 position)
        {
            Game.Entity.Spawn<BloodSplat>("blood_splat", position);
        }
    }
}