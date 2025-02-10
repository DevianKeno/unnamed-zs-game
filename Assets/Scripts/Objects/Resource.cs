using System.Collections.Generic;

using UZSG.Data;
using UZSG.Interactions;
using UZSG.Saves;

namespace UZSG.Objects
{
    public class Resource : BaseObject, IInteractable
    {
        public ResourceData ResourceData => objectData as ResourceData;
        public bool AllowInteractions { get; set; } = true;

        public string ActionText => "";
        public string DisplayName => ResourceData.DisplayNameTranslatable;

        public bool IsDamaged
        {
            get
            {
                if (Attributes.TryGet("health", out var health)) return !health.IsFull;
                return false;
            }
        }

        protected SaveData saveData;

        protected override void OnPlaceEvent()
        {
            LoadDefaultAttributes();
        }
        
        protected virtual void LoadDefaultAttributes()
        {
            attributes = new();
            attributes.AddList(objectData.Attributes);
        }
        
        public bool IsHarvestableBy(ToolData toolData)
        {
            return this.ResourceData != null
                && toolData != null
                && toolData.ToolType == ResourceData.ToolType;
        }

        public virtual List<InteractAction> GetInteractActions()
        {
            return new();
        }

        public virtual void Interact(InteractionContext context)
        {
            
        }
    }
}