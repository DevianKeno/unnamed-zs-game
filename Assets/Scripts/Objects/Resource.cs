using UZSG.Data;

namespace UZSG.Objects
{
    /// <summary>
    /// Represents naturally generated resources on the world (trees, pickups, ores, etc.).
    /// </summary>
    public class Resource : BaseObject, INaturallyPlaced
    {
        public ResourceData ResourceData => objectData as ResourceData;

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
    }
}