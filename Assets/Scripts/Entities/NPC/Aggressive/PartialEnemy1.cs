using System;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Players;
using UZSG.Systems;
using UZSG.Interactions;
using System.Collections;

namespace UZSG.Entities
{
    public partial class Enemy : Entity, IDetectable
    {
        EntityHitboxController hitboxes;

        void Awake()
        {
            hitboxes = GetComponent<EntityHitboxController>();
        }
        public void InitializeHitboxEvents()
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
                //SpawnDamageText(info.ContactPoint);
                Destroy(bullet.gameObject); /// Change on penetration
            }
            else if (info.Source.CollisionTag == "Melee")
            {
                SpawnBlood(info.ContactPoint);
                //SpawnDamageText(info.ContactPoint);
            }
        }

        void TakeDamage(float damage)
        {
            HealthAttri.Remove(damage);
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