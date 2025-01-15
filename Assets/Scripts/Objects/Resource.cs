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
    public class Resource : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public bool AllowInteractions { get; set; } = true;

        public string ActionText => "";
        public string DisplayName => ResourceData.DisplayName;

        public bool IsDamaged
        {
            get
            {
                if (Attributes.TryGet("health", out var health)) return !health.IsFull;
                return false;
            }
        }

        protected SaveData saveData;

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

        public List<InteractAction> GetInteractActions()
        {
            return new();
        }

        public void Interact(InteractionContext context)
        {
            
        }
    }
}