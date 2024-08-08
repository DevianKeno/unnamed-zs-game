namespace UZSG.Attributes
{
    public interface IAttributable
    {
        public AttributeCollection<Attribute> Attributes { get; }
    }
}