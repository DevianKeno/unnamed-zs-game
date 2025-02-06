using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Data;
using UZSG.Attributes;

namespace UZSG
{
    public class AttributesManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Contains the list of all attributes in the game.
        /// </summary>
        Dictionary<string, AttributeData> _attrDict = new();

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            LoadResources();
        }

        void LoadResources()
        {
            Game.Console.LogInfo("Reading data: Attributes...");
            var attrs = Resources.LoadAll<AttributeData>("Data/Attributes");
            foreach (var attr in attrs)
            {
                _attrDict[attr.Id] = attr;
            }
        }
        
        public AttributeData GetData(string id)
        {
            if (_attrDict.ContainsKey(id))
            {
                return _attrDict[id];
            }

            Game.Console.LogInfo($"Invalid Attribute Id '{id}'");
            return null;
        }
        
        
        public bool TryGetData(string id, out AttributeData data)
        {
            if (_attrDict.ContainsKey(id))
            {
                data = _attrDict[id];
                return true;
            }

            Game.Console.LogWarn($"Invalid Attribute Id '{id}'");
            data = null;
            return false;
        }
    }
}
