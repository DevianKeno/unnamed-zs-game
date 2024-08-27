using System;
using UZSG.Entities;

namespace UZSG.Interactions
{
    public struct InteractArgs
    {
    }

    /// <summary>
    /// Represents objects that the Player can interact with by pressing the Interact button (F by default).
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The action text displayed when looking at the Interactable.
        /// </summary>
        public abstract string Action { get; }
        /// <summary>
        /// The name of the object displayed when looking at the Interactable.
        /// </summary>
        public abstract string Name { get; }
        public bool AllowInteractions { get; set; }

        public event EventHandler<InteractArgs> OnInteract;

        public void Interact(IInteractActor actor, InteractArgs args);
        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
