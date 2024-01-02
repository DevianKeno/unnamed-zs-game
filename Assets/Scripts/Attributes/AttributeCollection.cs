using System;
using System.Collections.Generic;
using UZSG.Systems;

namespace UZSG.Attributes
{
    [Serializable]
    public struct AttributeIdPair
    {
        public string Id;
        public Attribute Attribute;
    }

    /// <summary>
    /// Represents a collection of attributes for an entity. Individual attributes can be indexed
    /// </summary>
    [Serializable]
    public class AttributeCollection
    {
        Dictionary<string, Attribute> _attributes = new();
        public Dictionary<string, Attribute> Attributes => _attributes;
        public List<AttributeIdPair> AttributeList = new();
        
        public Attribute this[string id]
        {
            get
            {
                if (Attributes.ContainsKey(id))
                {
                    return _attributes[id];
                }
                Game.Console?.Log($"Unable to retrieve Attribute [{id}] as it's not in the collection.");
                return null;
            }
        }

        public void AddAttribute(Attribute attribute)
        {
            if (!Attributes.ContainsKey(attribute.Data.Id))
            {
                Attributes[attribute.Data.Id] = attribute;
                AttributeList.Add(new()
                {
                    Id = attribute.Data.Id,
                    Attribute = attribute
                });
            } else
            {
                Game.Console?.Log($"Attribute [{attribute.Data.Id}] already exists within the collection.");
            }
        }

        public void RemoveAttribute(string id)
        {
            if (Attributes.TryGetValue(id, out Attribute attr))
            {
                Attributes.Remove(id, out Attribute attrs);
                var attrIdPair = AttributeList.Find(part => part.Id == id);
                AttributeList.Remove(attrIdPair);
            } else
            {
                Game.Console?.Log($"Unable to remove Attribute [{id}] as it does not exist within the collection.");
            }
        }

        public Attribute GetAttributeFromId(string id)
        {
            if (Attributes.ContainsKey(id))
            {
                return _attributes[id];
            }

            Game.Console?.Log($"Unable to retrieve Attribute [{id}] as it's not in the collection.");
            return null;
        }

        // public Attribute GetAttributeFromName(string name)
        // {
        //     // if (Attributes.ContainsKey(id))
        //     // {
        //     //     return _attributes[id];
        //     // } else // try searching for the name
        //     // {
        //     //     foreach (Attribute attr in _attributes.Values)
        //     //     {
        //     //         if (id != attr.Data.Name) continue;
        //     //         return attr;
        //     //     }
        //     //     Game.Console?.Log($"Unable to retrieve Attribute [{id}] as it's not in the collection.");
        //     //     return null;
        //     // }
        // }
    }
}
