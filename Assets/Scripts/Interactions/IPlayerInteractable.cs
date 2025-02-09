using UZSG.Entities;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that the Player can interact with by pressing the Interact button (F by default).
    /// </summary>
    public interface IPlayerInteractable : IInteractable
    {
        /// <summary>
        /// The actor that is currently interacting with the Interactable.
        /// </summary>
        public abstract Player Player { get; }
    }
}
