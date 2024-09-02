namespace UZSG.Interactions
{
    /// <summary>
    /// Base interface for information on more complex Interactions.
    /// </summary>
    public interface IInteractArgs
    {
        public IInteractable Interactable { get; set; }
        public IInteractActor Actor { get; set; }
    }
}