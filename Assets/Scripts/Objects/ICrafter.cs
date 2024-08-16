using UZSG.Crafting;

namespace UZSG.Objects
{
    public interface IPlaceable
    {
        public virtual void Place() { }
    }

    public interface ICrafter
    {
        public Crafter Crafter { get; }
    }
}