using System;
using URMG.Player;

namespace URMG.Interactions
{
    public struct InteractArgs
    {
    }

    public interface IInteractable 
    {
        public string Name { get; }
        /// <summary>
        /// Text display when hovered.
        /// </summary>
        public string Action { get; }
        public void Interact(PlayerActions actor, InteractArgs args);
        public event EventHandler<InteractArgs> OnInteract;
    }
}
