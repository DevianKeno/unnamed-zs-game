namespace UZSG.Interactions
{
    public struct InteractArgs : IInteractArgs
    {
        public IInteractable Interactable { get; set; }
        public IInteractActor Actor { get; set; }
    }
}