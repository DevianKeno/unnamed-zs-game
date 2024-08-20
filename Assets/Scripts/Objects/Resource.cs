using UZSG.Data;
using UZSG.Interactions;

namespace UZSG.Objects
{
    public class Resource : BaseObject, ICollisionTarget
    {
        public ResourceData ResourceData => objectData as ResourceData;
        
        /// On load on world
        protected override void Start()
        {
            base.Start();
        }
    }
}