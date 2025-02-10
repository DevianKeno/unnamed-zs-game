using System.Collections.Generic;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that the Player can interact with by pressing the Interact button (F by default).
    /// </summary>
    public interface IInteractable
    {
        public string DisplayName { get; }
        /// <summary>
        /// Whether to allow interactions for the Interactable.
        /// </summary>
        public bool AllowInteractions { get; set; }
        /// <summary>
        /// Get all actions from this Interactable.
        /// </summary>
        public virtual List<InteractAction> GetInteractActions() { return new(); }
        /// <summary>
        /// Called when an Actor interacts with the Interactable.
        /// </summary>
        public void Interact(InteractionContext context);

        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
