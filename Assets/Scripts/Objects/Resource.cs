using System;
using System.Collections.Generic;

using MEC;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Interactions;
using UZSG.Items.Tools;
using UZSG.Saves;
using UZSG.Systems;

namespace UZSG.Objects
{
    public class Resource : BaseObject, IInteractable, ILookable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public LookableType LookableType => LookableType.Resource;
        public bool AllowInteractions { get; set; } = true;

        public string Action => "";
        public string Name => ResourceData.Name;

        public bool IsDamaged
        {
            get
            {
                if (Attributes.TryGet("health", out var health)) return !health.IsFull;
                return false;
            }
        }

        protected SaveData saveData;

        public event EventHandler<IInteractArgs> OnInteract;

        protected override void Initialize()
        {
            base.Initialize();
            LoadDefaultAttributes();
        }
        
        protected virtual void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(objectData.Attributes);
        }
        
        public bool IsHarvestableBy(ToolData toolData)
        {
            return ResourceData != null && toolData != null && toolData.ToolType == ResourceData.ToolType;
        }

        public void Interact(IInteractActor actor, IInteractArgs args)
        {
            
        }
    }
}