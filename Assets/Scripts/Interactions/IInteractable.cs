using System;
using UZSG.Entities;

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
        public abstract string Action { get; }
        /// <summary>
        /// The name that may be displayed when looking at the Interactable.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Whether to allow interactions for the Interactable.
        /// </summary>
        public bool AllowInteractions { get; set; }
        /// <summary>
        /// Fired everytime an Actor interacts with the Interactable.
        /// </summary>
        public event EventHandler<IInteractArgs> OnInteract;

        /// <summary>
        /// Called when an Actor interacts with the Interactable.
        /// </summary>
        public void Interact(IInteractActor actor, IInteractArgs args);
        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
