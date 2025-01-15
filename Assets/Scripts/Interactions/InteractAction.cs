using System;

using UnityEngine.InputSystem;

namespace UZSG.Interactions
{
    public enum InteractActionType {
        InteractWith, PickUp, Use, Equip
    }

    public class InteractAction
    {
        /// <summary>
        /// The object to perform the interaction with.
        /// </summary>
        public IInteractable Interactable { get; set; }
        /// <summary>
        /// The entity who performs the interaction.
        /// </summary>
        public IInteractActor Actor { get; set; }
        /// <summary>
        /// The action text that may be displayed when looking at the Interactable.
        /// </summary>
        public string ActionText { get; set; }
        /// <summary>
        /// The name of the interactable to be displayed.
        /// </summary>
        public string InteractableText { get; set; }
        /// <summary>
        /// Whether if the input requires holding down the key.
        /// </summary>
        public bool IsHold { get; set; }
        public float HoldDurationSeconds { get; set; }
        public InputAction InputAction { get; set;}
        /// <summary>
        /// Called when the Interact Action is performed.
        /// </summary>
        public event Action<InteractionContext> OnPerformed;

        public void Perform(InteractionContext context)
        {
            OnPerformed?.Invoke(context);
        }
    }
}