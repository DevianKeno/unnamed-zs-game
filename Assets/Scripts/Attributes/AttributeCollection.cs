using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Saves;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a collection of attributes for an entity.
    /// For performance, you can cache an Attribute first, then subscribe to events for tracking changes in its value.
    /// Individual attributes can also be indexed.
    /// </summary>
    [Serializable]
    public class AttributeCollection : ISaveDataReadWrite<List<AttributeSaveData>>
    {
        /// Just so can view in Inspector
        [SerializeField] List<Attribute> attributes = new();
        Dictionary<string, Attribute> _attrsDict = new();
        public List<Attribute> Attributes
        {
            get => _attrsDict.Values.ToList();
        }

        public Attribute this[string id]
        {
            get => Get(id);
        }

        public void ReadSaveJson(List<AttributeSaveData> saveData)
        {
            foreach (var attrSaveData in saveData)
            {
                if (Game.Attributes.TryGetData(attrSaveData.Id, out var attrData))
                {
                    var newAttr = new Attribute(attrData);
                    newAttr.ReadSaveJson(attrSaveData);
                    Add(newAttr);
                }
                else
                {
                    Game.Console.LogAndUnityLog($"Tried to retrieve Attribute '{attrSaveData.Id}', but it does not exists.");
                }
            }
        }
        
        public List<AttributeSaveData> WriteSaveJson()
        {
            var saveData = new List<AttributeSaveData>();

            foreach (var attr in _attrsDict.Values.ToList())
            {
                saveData.Add(attr.WriteSaveJson());
            }
            
            return saveData;
        }

        public void Add(Attribute attribute)
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

        public void AddList(List<Attribute> attributes)
        {
            foreach (var attr in attributes)
            {
                if (_attrsDict.TryGetValue(attr.Id, out var xattr))
                {
                    Game.Console.LogWarning($"Duplicate attribute found '{attr.Id}', disregarding...");
                    continue;
                }
                _attrsDict[attr.Id] = attr;
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

        public bool Remove(string id, out Attribute attribute)
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

        public Attribute Get(string id)
        {
            if (_attrsDict.ContainsKey(id))
            {
                return _attrsDict[id];
            }

            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return null;
        }
        
        public T Get<T>(string id) where T : Attribute
        {
            if (_attrsDict.ContainsKey(id))
            {
                return (T) _attrsDict[id];
            }

            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return null;
        }

        public bool TryGet(string id, out Attribute attribute)
        {
            if (_attrsDict.ContainsKey(id))
            {
                attribute = _attrsDict[id];
                return true;
            } 
            
            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            attribute = Attribute.None;
            return false;
        }

        public bool TryGet<T>(string id, out T attribute) where T : Attribute
        {
            if (_attrsDict.ContainsKey(id))
            {
                attribute = (T) _attrsDict[id];
                return true;
            } 
            
            Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            attribute = (T) Attribute.None;
            return false;
        }
    }
}
