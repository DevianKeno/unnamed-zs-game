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
            Game.Console.Log("Initializing Attribute database...");
            var attrs = Resources.LoadAll<AttributeData>("Data/Attributes");
            foreach (var attr in attrs)
            {
                _attributesDict[attr.Id] = attr;
            }

            // Addressables.LoadAssetsAsync<AttributeData>(assetLabelReference, (a) =>
            // {
            //     Game.Console?.LogDebug($"Loading data for Attribute {a.Id}");
            //     _attributeList[a.Id] = a;
            // });
        }
    }
}
