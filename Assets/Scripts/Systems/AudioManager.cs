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
        
        AudioSource InstantiateAudioSource()
        {
            return Instantiate(audioSourcePrefab, transform).GetComponent<AudioSource>();
        }

        
        #region Public methods

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

        public async void LoadAudioAssets(AudioAssetsData data)
        {
            await LoadAudioAssetsAsync(data.AudioClips, (result) =>
            {
                foreach (var item in result)
                {
                    if (!_audioClipsDict.ContainsKey(item.name))
                    {
                        _audioClipsDict[item.name] = item;
                    }
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

        public void Play(string name)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource(clip);
            }
        }

        public void PlayInWorld(string name, Vector3 position)
        {
            if (_audioClipsDict.TryGetValue(name, out var clip))
            {
                PlayClipAsNewSource3D(clip, position);
            }
        }
        
        public void PlayClipAsNewSource(AudioClip clip)
        {
            var source = InstantiateAudioSource();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.Play();
            Destroy(source, source.clip.length);
        }

        public void PlayClipAsNewSource3D(AudioClip clip, Vector3 position)
        {
            var source = InstantiateAudioSource();
            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = 1f;
            source.Play();
            Destroy(source, source.clip.length);
        }

        #endregion
    }
}
