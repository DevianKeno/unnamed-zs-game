using System;
using System.Collections.Generic;
using UZSG.Systems;
using UZSG.Data;
using UnityEngine;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a collection of attributes for an entity.
    /// For performance, you can cache an Attribute first, then subscribe to events for tracking changes in its value.
    /// Individual attributes can also be indexed.
    /// </summary>
    [Serializable]
    public class AttributeCollection<T> where T : Attribute
    {
        public List<T> Attributes = new();
        Dictionary<string, T> _attributesDict = new();
        
        public T this[string id]
        {
            get => GetAttribute(id);
        }

        /// <summary>
        /// Just a way to combine grouped attributes.
        /// </summary>
        public AttributeCollection(AttributeCollection<T> collection)
        {
            Attributes = collection.Attributes;
        }

        /// <summary>
        /// Just a way to combine grouped attributes.
        /// </summary>
        public AttributeCollection(AttributeCollection<T>[] attrCollections)
        {
            foreach (var collection in attrCollections)
            {
                Attributes.AddRange(collection.Attributes);
            }
        }

        public void Init()
        {
            foreach (T attr in Attributes)
            {                
                AddAttribute(attr);
                attr.Init();
            }
        }

        public void LoadData(AttributeCollectionData<T> data)
        {
            //
        }
        
        public void SaveData(AttributeCollectionData<T> data)
        {
            // 
        }

        public void AddAttribute(T attribute)
        {
            if (!attribute.IsValid) return;

            if (!_attributesDict.ContainsKey(attribute.Data.Id))
            {
                _attributesDict[attribute.Data.Id] = attribute;
            }
            else
            {
                Game.Console.Log($"Attribute [{attribute.Data.Id}] already exists within the collection.");
            }
        }

        public void RemoveAttribute(string id)
        {
            if (_attributesDict.ContainsKey(id))
            {
                _attributesDict.Remove(id);
            }
            else
            {
                Game.Console.Log($"Unable to remove Attribute [{id}] as it does not exist within the collection.");
            }
        }

        public bool RemoveAttribute(string id, out T attribute)
        {
            if (_attributesDict.ContainsKey(id))
            {
                _attributesDict.Remove(id, out attribute);
                return true;
            }
            else
            {
                attribute = null;
                Game.Console.Log($"Unable to remove Attribute [{id}] as it does not exist within the collection.");
                return false;
            }
        }

        public T GetAttribute(string id)
        {
            if (_attributesDict.ContainsKey(id))
            {
                return _attributesDict[id];
            }
            else
            {
                string msg = $"Unable to retrieve Attribute [{id}] as it's not in the collection.";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
                return null;
            }
        }

        public bool TryGetAttribute(string id, out Attribute attribute)
        {
            if (_attributesDict.ContainsKey(id))
            {
                attribute = _attributesDict[id];
                return true;
            } 
            else
            {
                attribute = Attribute.None;
                string msg = $"Unable to retrieve Attribute [{id}] as it's not in the collection.";
                Game.Console.Log(msg);
                Debug.LogWarning(msg);
                return false;
            }
        }

        public void Combine(AttributeCollection<T> other)
        {
            Attributes.AddRange(other.Attributes);
        }
    }
}
