using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Systems
{
    public class AudioManager : MonoBehaviour, IInitializable
    {        
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, AudioClip> _audioClipsDict = new();
        [SerializeField] AssetLabelReference assetLabelReference;

        [SerializeField] AudioSource Global;
        
        internal void Initialize()
        {
            Game.Console.LogDebug("Initializing Audio database...");
            var clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (AudioClip clip in clips)
            {
                _audioClipsDict[clip.name] = clip;
            }
        }


        #region Public methods

        public void Play(string name, bool newSource = false)
        {
            if (_audioClipsDict.ContainsKey(name))
            {
                Global.clip = _audioClipsDict[name];
                Global.Play();
            }
        }
        
        #endregion
    }
}
