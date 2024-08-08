using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;

namespace UZSG.Systems
{
    public class AudioManager : MonoBehaviour, IInitializeable
    {        
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, AudioClip> _audioClipsDict = new();
        
        [SerializeField] AudioSource Global;
        [SerializeField] GameObject audioSourcePrefab;
        
        internal void Initialize()
        {
            Game.Console.Log("Reading data: Audio...");
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

        public async void LoadAudioAssets(List<AssetReference> assets)
        {
            await LoadAudioAssetsAsync(assets, (result) =>
            {
                foreach (var item in result)
                {
                    _audioClipsDict[item.name] = item;;
                }
            });
        }

        public async Task LoadAudioAssetsAsync(List<AssetReference> assets, Action<List<AudioClip>> onComplete = null)
        {
            var audioClips = new List<AudioClip>();
            var loadTasks = new List<Task>();

            foreach (var asset in assets)
            {
                var tcs = new TaskCompletionSource<AudioClip>();
                loadTasks.Add(tcs.Task);
                Addressables.LoadAssetAsync<AudioClip>(asset).Completed += (a) =>
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

        public async Task LoadAudioAssets(List<AssetReference> assetReference, Action<List<AudioClip>> onComplete = null)
        {
            var audioClips = new List<AudioClip>();
            var loadTasks = new List<Task>();

            foreach (var audioAssetId in assetReference)
            {
                var tcs = new TaskCompletionSource<AudioClip>();
                loadTasks.Add(tcs.Task);

                Addressables.LoadAssetAsync<AudioClip>(audioAssetId).Completed += (a) =>
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

        Dictionary<string, AudioSource> _playingAudios = new();

        public void Play(string name)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                Global.clip = clip;
                Global.Play();
            }
        }
        
        public void Play(string name, Vector3 position)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                Global.clip = _audioClipsDict[name];
                Global.Play();
            }
        }
        
        public void Play(string name, Vector3 position, Transform parent)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                Global.clip = _audioClipsDict[name];
                Global.Play();
            }
        }

        public void CreateAudioPool()
        {

        }

        public void PlaySolo(string name, bool restart = false)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (!_audioClipsDict.TryGetValue(name, out var newClip))
            {
                Game.Console.Log($"[Audio]: The audio '{name}' does not exists.");
                return;
            }
            if (_playingAudios.TryGetValue(name, out var audioSource) && !restart)
            {
                Game.Console.Log($"[Audio]: The audio '{name}' is currently playing");
                return;
            }
            
            audioSource.Stop();
            Destroy(audioSource.gameObject);
            audioSource = InstantiateAudioSource();
            audioSource.clip = newClip;
            audioSource.spatialBlend = 0;
            audioSource.Play();
        }

        AudioSource InstantiateAudioSource()
        {
            return Instantiate(audioSourcePrefab, transform).GetComponent<AudioSource>();
        }
        
        #endregion
    }
}
