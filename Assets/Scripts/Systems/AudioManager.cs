using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Systems
{
    public class AudioManager : MonoBehaviour, IInitializable
    {        
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, AudioClip> _audioList = new();
        [SerializeField] AssetLabelReference assetLabelReference;
        
        internal void Initialize()
        {        
        }
    }
}
