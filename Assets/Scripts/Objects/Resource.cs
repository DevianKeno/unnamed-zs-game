using System;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Items.Tools;

namespace UZSG.Objects
{
    public class Resource : BaseObject
    {
        public ResourceData ResourceData => objectData as ResourceData;

        public float Health = 100;
        
        public event EventHandler<CollisionHitInfo> OnHit;

        void Start()
        {

        }

        public void HitBy(CollisionHitInfo other)
        {
            if (other.By is HeldToolController tool)
            {
                Health -= tool.ToolData.Attributes["efficiency"].Value;
            }
        }
    }
}