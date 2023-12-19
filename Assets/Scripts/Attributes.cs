using System.Collections.Generic;

namespace URMG
{
    /// <summary>
    /// Represents a collection of attributes for an entity.
    /// </summary>
    public class Attributes
    {
        protected Dictionary<string, Attribute> _attributes;
        
        public Attribute this[string name]
        {
            get => _attributes[name];
        }

        public void AddAttribute(Attribute attribute)
        {
            // _attributes.Add(attribute.Name, attribute);
        }
    }
}
