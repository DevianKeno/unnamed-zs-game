using System;
using System.Linq;
using System.Reflection;
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

        public void ReadSaveJSON<U>(List<U> saveData) where U : AttributeSaveData
        {
            foreach (U attr in saveData)
            {
                if (Game.Attributes.TryGetData(attr.Id, out var attrData))
                {
                    Type t = typeof(T);
                    var constructor = t.GetConstructor(new Type[] { typeof(AttributeData) });
                    if (constructor == null)
                    {
                        Debug.LogError($"No constructor found for type {t.Name} with a single parameter of type AttributeData");
                        continue;
                    }
                    T newAttr = (T) constructor.Invoke(new object[] { attrData });
                    newAttr.ReadSaveData(attr);
                    Add(newAttr);
                }
            }
        }
        
        public void WriteSaveJSON(List<T> data)
        {
            foreach (T attr in attributes)
            {
                //
            }
        }

        public void Add(T attribute)
        {
            if (!attribute.IsValid) return;

            if (!_attrsDict.ContainsKey(attribute.Data.Id))
            {
                _attrsDict[attribute.Data.Id] = attribute;
                attributes.Add(attribute);
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
                attributes.Remove(_attrsDict[id]);
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
                attributes.Remove(_attrsDict[id]);
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
            
            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            attribute = (T) Attribute.None;
            return false;
        }

        public void InitializeFromData(List<GenericAttributeSaveData> data)
        {
        }
    }
}
