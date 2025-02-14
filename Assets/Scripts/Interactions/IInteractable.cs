using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents objects that can be interacted with by pressing the Interact button (F by default).
    /// </summary>
    public interface IInteractable
    {
        public GameObject gameObject { get; }
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
        public virtual void Interact(InteractionContext context) { }

        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
