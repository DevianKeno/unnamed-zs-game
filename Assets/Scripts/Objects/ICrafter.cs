using UZSG.Crafting;

namespace UZSG.Objects
{
    /// <summary>
    /// Represents stuff that has a Crafter logic attached to it.
    /// </summary>
    public interface ICrafter
    {
        public Crafter Crafter { get; }
    }
}