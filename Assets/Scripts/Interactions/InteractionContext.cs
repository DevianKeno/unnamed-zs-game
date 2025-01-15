namespace UZSG.Interactions
{
    public struct InteractionContext
    {
        public string Action { get; set; }
        /// <summary>
        /// The object to perform the interaction with.
        /// </summary>
        public IInteractable Interactable { get; set; }
        /// <summary>
        /// The entity who performs the interaction.
        /// </summary>
        public IInteractActor Actor { get; set; }
        public InteractPhase Phase { get; set; }
    }
}