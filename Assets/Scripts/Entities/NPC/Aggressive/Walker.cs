using UnityEngine;

using UZSG.Interactions;
using UZSG.Items.Weapons;

namespace UZSG.Entities
{
    /// <summary>
    /// Walker class zombie.
    /// </summary>
    public class Walker : Enemy
    {
        protected override void OnHitboxCollision(object sender, HitboxCollisionInfo info)
        {
            if (info.Source is Bullet bullet)
            {
                var hitbox = sender as Hitbox;

                TakeDamage(10f);

                SpawnBlood(info.ContactPoint);
                // SpawnDamageText(info.ContactPoint);
                Destroy(bullet.gameObject); /// Change on penetration
            }
            else if (info.Source is MeleeWeaponController meleeWeapon)
            {
                TakeDamage(10f);

                SpawnBlood(info.ContactPoint);
                // SpawnDamageText(info.ContactPoint);
            }
        }
    }
}