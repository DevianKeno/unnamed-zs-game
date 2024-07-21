using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Attributes;

namespace UZSG.Systems
{
    public class AttributesManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Contains the list of all attributes in the game.
        /// </summary>
        Dictionary<string, AttributeData> _attributesDict = new();
        [SerializeField] AssetLabelReference assetLabelReference;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var startTime = Time.time;
            LoadResources();
        }

        void LoadResources()
        {
            Game.Console.Log("Initializing Attribute database...");
            var attrs = Resources.LoadAll<AttributeData>("Data/attributes");
            foreach (var attr in attrs)
            {
                _attributesDict[attr.Id] = attr;
            }
        }

        public Attribute Create(string id)
        {
            if (_attributesDict.ContainsKey(id))
            {
                return new Attribute(_attributesDict[id]);
            }
            return Attribute.None;
        }
    }
}
