using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Items.Weapons;

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

        public struct LoadAudioAssetContext
        {
            public AudioClip AudioClip { get; set; }
        }

        public delegate void OnLoadAudioAssetCompleted(LoadAudioAssetContext context);

        public void LoadAudioAsset(AssetReference assetReference, OnLoadAudioAssetCompleted completed = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(assetReference).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    completed?.Invoke(new()
                    {
                        AudioClip = a.Result,
                    });
                }
            };
        }
        
        public async Task LoadAudioAssets(List<AudioAssetId> assetReference, Action<List<AudioClip>> onComplete = null)
        {
            var audioClips = new List<AudioClip>();
            var loadTasks = new List<Task>();

            foreach (var audioAssetId in assetReference)
            {
                var tcs = new TaskCompletionSource<AudioClip>();
                loadTasks.Add(tcs.Task);

                Addressables.LoadAssetAsync<AudioClip>(audioAssetId.AudioAsset).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        audioClips.Add(a.Result);
                        tcs.SetResult(a.Result);
                    }
                };
            }
            await Task.WhenAll(loadTasks);
            onComplete?.Invoke(audioClips);
        }

        
        #region Public methods

        public void Play(string name)
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
