using UZSG.Interactions;

namespace UZSG.Objects
{
    /// <summary>
    /// The name says it all.
    /// </summary>
    public interface IPickupable
    {
        public void Pickup(IInteractActor actor);
    }
}