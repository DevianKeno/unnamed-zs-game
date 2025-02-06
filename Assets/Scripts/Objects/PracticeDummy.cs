using System;

using UnityEngine;

using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Interactions;


namespace UZSG.Objects
{
    public class PracticeDummy : BaseObject, IAttributable, ICollisionTarget
    {
        [SerializeField] GameObject model;
        
        /// OnPlace!
        protected override void Start()
        {
            base.Start();
        }
        
        public void AttackedBy(HitboxCollisionInfo info)
        {
            if (info.ObjectType == ObjectCollisionType.Melee)
            {
                OnHitMelee(info);
            }
            else if (info.ObjectType == ObjectCollisionType.Projectile)
            {
                OnHitProjectile(info);
            }
        }

        void AnimateHit()
        {
            LeanTween.cancel(model);
            LeanTween.rotateX(model, -5f, 1f)
            .setEaseOutElastic();
        }

        void SpawnDamageText(Vector3 position)
        {
            Game.Entity.Spawn<DamageText>("damage_text", position, (info) =>
            {
                info.Entity.Text = "15";
            });
        }

        void OnHitMelee(HitboxCollisionInfo other)
        {
            AnimateHit();
            SpawnDamageText(other.ContactPoint);
        }

        void OnHitProjectile(HitboxCollisionInfo other)
        {
            SpawnDamageText(other.ContactPoint);
        }
    }
}