using System;

using UnityEngine.InputSystem;

namespace UZSG.Interactions
{
    public class InteractAction
    {
        public InteractType Type { get; set; }
        /// <summary>
        /// The object to perform the interaction with.
        /// </summary>
        public IInteractable Interactable { get; set; }
        /// <summary>
        /// The entity who performs the interaction.
        /// </summary>
        public IInteractActor Actor { get; set; }
        /// <summary>
        /// Whether if the input requires holding down the input key.
        /// </summary>
        public bool IsHold { get; set; }
        public float HoldDurationSeconds { get; set; }
        public InputAction InputAction { get; set;}
        /// <summary>
        /// Called when the Interact Action is performed.
        /// </summary>
        public event Action<InteractionContext> OnPerformed;

        public virtual void Perform(InteractionContext context)
        {
            OnPerformed?.Invoke(context);
        }

        public static string Translatable(InteractType type)
        {
            return type switch
            {
                InteractType.Interact => Game.Locale.Translatable("action.interact"),
                InteractType.Use => Game.Locale.Translatable("action.use"),
                InteractType.PickUp => Game.Locale.Translatable("action.pick_up"),
                InteractType.Open => Game.Locale.Translatable("action.open"),
                InteractType.Close => Game.Locale.Translatable("action.close"),
                InteractType.Enter => Game.Locale.Translatable("action.enter"),
                InteractType.Exit => Game.Locale.Translatable("action.exit"),
                _ => "Interact",
            };
        }
    }
}