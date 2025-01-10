namespace UZSG.Interactions
{
    public enum InteractPhase {
        Started, Finished, Canceled, 
    }
    
    public struct InteractContext : IInteractArgs
    {
        public IInteractable Interactable { get; set; }
        public IInteractActor Actor { get; set; }
        public InteractPhase Phase { get; set; }
    }
}