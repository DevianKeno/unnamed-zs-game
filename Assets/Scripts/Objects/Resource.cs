using UZSG.Data;
using UZSG.Interactions;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class Resource : BaseObject, ICollisionTarget
    {
        public ResourceData ResourceData => objectData as ResourceData;

        protected SaveData saveData;
        
        /// On load on world
        protected override void Start()
        {
            base.Start();
            
            LoadDefaultAttributes();
        }
        
        protected virtual void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(objectData.Attributes);
        }
    }
}