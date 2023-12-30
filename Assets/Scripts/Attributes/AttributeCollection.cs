using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a collection of attributes for an entity.
    /// </summary>
    [Serializable]
    public abstract class AttributeCollection
    {
        public abstract Dictionary<string, Attribute> Attributes { get; }
        
        public abstract Attribute this[string name] { get; }

        public virtual void AddAttribute(string name, Attribute attribute)
        {
            if (!Attributes.ContainsKey(name))
            {
                Attributes.Add(name, attribute);
            } else
            {
                Debug.LogWarning($"Attribute [{name}] already exists within collection.");
            }
        }
    }
}
