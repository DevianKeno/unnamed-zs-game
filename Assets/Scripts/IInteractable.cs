using System;
using UnityEngine;
using UZSG.Players;

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
        public string Name { get; }
        /// <summary>
        /// Text displayed when looking at the Interactable.
        /// </summary>
        public string ActionText { get; }
        public event EventHandler<InteractArgs> OnInteract;
        public void Interact(PlayerActions actor, InteractArgs args);
        public virtual void OnLookEnter() { }
        public virtual void OnLookExit() { }
    }
}
