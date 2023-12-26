using System;
using UnityEngine;
using UZSG.Player;

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
        public abstract string Name { get; }
        /// <summary>
        /// Text displayed when looking at the Interactable.
        /// </summary>
        public abstract string Action { get; }
        public abstract void Interact(PlayerActions actor, InteractArgs args);
        public event EventHandler<InteractArgs> OnInteract;
    }
}
