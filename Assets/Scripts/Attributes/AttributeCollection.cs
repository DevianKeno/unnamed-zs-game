using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

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
        /// Just so can view in Inspector
        [SerializeField] List<T> attributes = new();
        Dictionary<string, T> _attrsDict = new();
        public List<T> Attributes
        {
            get => _attrsDict.Values.ToList();
        }

        public T this[string id]
        {
            get => Get(id);
        }

        public void ReadAttributesData(List<AttributeData> data)
        {
            foreach (var attr in data)
            {
                Attribute newAttr = attr.Type switch /// wdym it's a fucking enum
                {
                    AttributeType.Generic => new GenericAttribute(attr.Id),
                    AttributeType.Vital => new VitalAttribute(attr.Id),
                };

                Add((T) newAttr);
                attributes.Add((T) newAttr);
            }
        }
    
        // public void LoadData(AttributeCollectionData data)
        // {
        //     //
        // }
        
        // public void SaveData(AttributeCollectionData data)
        // {
        //     // 
        // }

        public void Add(T attribute)
        {
            if (!attribute.IsValid) return;

            if (!_attrsDict.ContainsKey(attribute.Data.Id))
            {
                _attrsDict[attribute.Data.Id] = attribute;
            }
            else
            {
                Game.Console.LogAndUnityLog($"Attribute [{attribute.Data.Id}] already exists within the collection.");
            }
        }

        public void Remove(string id)
        {
            if (_attrsDict.ContainsKey(id))
            {
                _attrsDict.Remove(id);
            }
            else
            {
                Game.Console.LogAndUnityLog($"Unable to remove Attribute [{id}] as it does not exist within the collection.");
            }
        }

        public bool Remove(string id, out T attribute)
        {
            if (_attrsDict.ContainsKey(id))
            {
                _attrsDict.Remove(id, out attribute);
                return true;
            }

            attribute = null;
            Game.Console.LogAndUnityLog($"Unable to remove Attribute '{id}' as it does not exist within the collection.");
            return false;
        }

        public T Get(string id)
        {
            if (_attrsDict.ContainsKey(id))
            {
                return _attrsDict[id];
            }

            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return null;
        }

        public bool TryGet(string id, out T attribute)
        {
            if (_attrsDict.ContainsKey(id))
            {
                attribute = _attrsDict[id];
                return true;
            } 
            
            attribute = (T) Attribute.None;
            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return false;
        }
    }
}
