using System;

using UnityEngine;
using UZSG.Attributes;
using UZSG.Interactions;

namespace UZSG.Objects
{
    public class PracticeDummy : BaseObject, IAttributable, ICollisionTarget
    {
        public event EventHandler<CollisionHitInfo> OnHit;

        [SerializeField] GameObject model;
        
        /// OnPlace!
        protected override void Start()
        {
            base.Start();
        }
        
        public void HitBy(CollisionHitInfo other)
        {
            if (other.Type == CollisionType.Melee)
            {
                OnHitMelee(other);
            }
            else if (other.Type == CollisionType.Projectile)
            {
                OnHitProjectile(other);
            }
        }

        void AnimateHit()
        {
            LeanTween.cancel(model);
            LeanTween.rotateX(model, -5f, 1f)
            .setEaseOutElastic();
        }

        void OnHitMelee(CollisionHitInfo other)
        {
            AnimateHit();
        }

        void OnHitProjectile(CollisionHitInfo other)
        {
            
        }
    }
}