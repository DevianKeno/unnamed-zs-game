using System;

using UnityEngine;

using UZSG.Interactions;

namespace UZSG.Objects
{
    public class PracticeDummy : BaseObject, ICollisionTarget
    {
        public event EventHandler<CollisionHitInfo> OnHit;
        
        public void HitBy(CollisionHitInfo other)
        {

        }
    }
}