using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Entities
{
    public class Skinwalker : Entity
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

        void OnCollision(object sender, Collider collider)
        {
            if (collider.gameObject == null) return;

            if (collider.CompareTag("Bullet"))
            {
                if (collider.gameObject.TryGetComponent<Bullet>(out var bullet))
                {
                    var hitbox = sender as Hitbox;
                    Debug.Log($"Shot part {hitbox.Part}");
                    var target = bullet.transform.position;
                    Game.Entity.Spawn("blood_splat", (entity) =>
                    {
                        entity.Entity.transform.position = target;
                    });
                    Destroy(bullet.gameObject);
                }
            }   
        }
    }
}