using System;
using System.Collections.Generic;

using MEC;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class Resource : BaseObject, ILookable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public LookableType LookableType => LookableType.Resource;
        public bool AllowInteractions { get; set; } = true;

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