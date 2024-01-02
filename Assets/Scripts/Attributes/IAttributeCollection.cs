namespace UZSG.Attributes
{
    /// <summary>
    /// Represents objects that have a collection of attributes.
    /// </summary>
    public interface IAttributeCollection
    {
        public AttributeCollection Attributes { get; }
    }
}
