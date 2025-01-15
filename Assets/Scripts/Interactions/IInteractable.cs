using System;
using System.Collections.Generic;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that the Player can interact with by pressing the Interact button (F by default).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The action text that may be displayed when looking at the Interactable.
        /// </summary>
        public abstract string ActionText { get; }
        /// <summary>
        /// The name that may be displayed when looking at the Interactable.
        /// </summary>
        public abstract string DisplayName { get; }
        /// <summary>
        /// Whether to allow interactions for the Interactable.
        /// </summary>
        public bool AllowInteractions { get; set; }

        /// <summary>
        /// Get all actions from this Interactable.
        /// </summary>
        public List<InteractAction> GetInteractActions();
        /// <summary>
        /// Called when an Actor interacts with the Interactable.
        /// </summary>
        public void Interact(InteractionContext context);
        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
