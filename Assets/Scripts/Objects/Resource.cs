using System;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Interactions;
using UZSG.Items;
using UZSG.Items.Tools;
using UZSG.Systems;

namespace UZSG.Objects
{
    public class Resource : BaseObject, ICollisionTarget
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public event EventHandler<CollisionHitInfo> OnHit;
        
        /// On load on world
        protected override void Start()
        {
            base.Start();
            
            Attributes.Add(new GenericAttribute("health"));
            Attributes["health"].Value = 10f;
        }
        
        public virtual void HitBy(CollisionHitInfo other)
        {
        }
    }
}