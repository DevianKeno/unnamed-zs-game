using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Saves;

namespace UZSG.Attributes
{
    /// <summary>
    /// Represents a collection of Attributes for an Object.
    /// For performance, you can cache an Attribute first, then subscribe to events for tracking changes in its value.
    /// Individual attributes can also be indexed.
    /// </summary>
    [Serializable]
    public class AttributeCollection : IEnumerable<Attribute>, ISaveDataReadWrite<List<AttributeSaveData>>
    {
        Dictionary<string, Attribute> _attrsDict = new();
        public Dictionary<string, Attribute> AttributesDict => _attrsDict;

        public Attribute this[string id]
        {
            get => Get(id);
        }

        
        #region Saves read/write

        public void ReadSaveData(List<AttributeSaveData> saveData)
        {
            foreach (var attrSaveData in saveData)
            {
                var id = attrSaveData.Id.Trim().ToLower();
                
                if (_attrsDict.TryGetValue(id, out var attr))
                {
                    /// overwrite existing
                    attr.ReadSaveData(attrSaveData);
                }
                else
                {
                    if (Game.Attributes.TryGetData(id, out var attrData))
                    {
                        var newAttr = new Attribute(attrData);
                        newAttr.ReadSaveData(attrSaveData);
                        Add(newAttr);
                    }
                }
            }
        }
        
        public List<AttributeSaveData> WriteSaveData()
        {
            var saveData = new List<AttributeSaveData>();

            foreach (var attr in _attrsDict.Values.ToList())
            {
                saveData.Add(attr.WriteSaveData());
            }
            
            return saveData;
        }

        #endregion


        public void Add(Attribute attribute)
        {
            if (!attribute.IsValid) return;
            
            if (!_attrsDict.ContainsKey(attribute.Data.Id))
            {
                _attrsDict[attribute.Data.Id] = attribute;
            }
            else
            {
                // Game.Console.LogAndUnityLog($"Attribute [{attribute.Data.Id}] already exists within the collection.");
            }
        }

        public void AddList(List<Attribute> attributes)
        {
            foreach (var attr in attributes)
            {
                if (_attrsDict.TryGetValue(attr.Id, out var xattr))
                {
                    Game.Console.LogWarn($"Duplicate attribute found '{attr.Id}', disregarding...?");
                    continue;
                }

                _attrsDict[attr.Id] = new(attr);
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
                // Game.Console.LogAndUnityLog($"Unable to remove Attribute [{id}] as it does not exist within the collection.");
            }
        }

        public bool Remove(string id, out Attribute attribute)
        {
            if (_attrsDict.ContainsKey(id))
            {
                _attrsDict.Remove(id, out attribute);
                return true;
            }

            attribute = null;
            // Game.Console.LogAndUnityLog($"Unable to remove Attribute '{id}' as it does not exist within the collection.");
            return false;
        }

        /// <summary>
        /// Returns null if Attribute of Id is not present in this collection. Handle well :) or use TryGet
        /// </summary>
        public Attribute Get(string id)
        {
            if (_attrsDict.ContainsKey(id))
            {
                return _attrsDict[id];
            }

            // Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return null;
        }
        
        /// <summary>
        /// Returns null if Attribute of Id is not present in this collection. Handle well :) or use TryGet
        /// </summary>
        public T Get<T>(string id) where T : Attribute
        {
            if (_attrsDict.ContainsKey(id))
            {
                return (T) _attrsDict[id];
            }

            // Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            return null;
        }

        public bool TryGet(string id, out Attribute attribute)
        {
            if (_attrsDict.ContainsKey(id))
            {
                attribute = _attrsDict[id];
                return true;
            } 
            
            // Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' from '' as it's not in the collection.");
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
            
            // Game.Console.LogAndUnityLog($"Unable to retrieve Attribute '{id}' as it's not in the collection.");
            attribute = (T) Attribute.None;
            return false;
        }
        
        public List<Attribute> AsList()
        {
            return _attrsDict.Values.ToList();
        }

        public IEnumerator<Attribute> GetEnumerator()
        {
            return _attrsDict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
