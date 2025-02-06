using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MEC;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Data;

namespace UZSG
{
    public class AudioManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        [Range(0, 1), SerializeField] float soundVolume = 1f;
        public float SoundVolume
        {
            get => soundVolume;
            set
            {
                soundVolume = Mathf.Clamp01(value);
                SetSoundVolume(soundVolume);
            }
        }
        [Range(0, 1), SerializeField] float musicVolume = 15f;
        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                SetMusicVolume(value);
            }
        }
        [Range(0, 1)] public float ambianceVolume = 1f;
        [SerializeField] bool allowTrackPolyphony = false;

        Dictionary<string, AudioClip> _audioClipsDict = new();
        [SerializeField] List<TrackList> trackLists;
        Dictionary<string, AudioClip> _trackClipsDict = new();

        /// <summary>
        /// Dict of currently playing audio clips.
        /// </summary>
        HashSet<AudioSource> _playingAudioSources = new();
        /// <summary>
        /// Dict of currently playing tracks.
        /// <c>string</c> is its file name; AudioSource is component instance.
        /// </summary>
        Dictionary<string, AudioSource> _playingTrackSources = new();
    
#if UNITY_EDITOR

        void OnValidate()
        {
            SetMusicVolume(musicVolume);
            SetSoundVolume(musicVolume);
        }
#endif

        internal void Initialize()
        {
            Game.Console.LogInfo("Reading data: Audio...");
            var clips = Resources.LoadAll<AudioClip>("Audio");
            foreach (AudioClip clip in clips)
            {
                _audioClipsDict[clip.name] = clip;
            }
            
            /// Tracks
            foreach (TrackList trackList in trackLists)
            {
                foreach (AudioClip clip in trackList.AudioClips)
                {
                    _trackClipsDict[clip.name] = clip;
                }
            }
        }


        #region Public methods


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

        public void PlayInWorld(AudioClip audioClip, Vector3 position)
        {
            PlayClipAsNewSource3D(audioClip, position);
        }

        public void PlayTrack(string id, float volume, bool loop = true)
        {
            if (_trackClipsDict.TryGetValue(id, out var clip))
            {
                if (_playingTrackSources.ContainsKey(id) && !allowTrackPolyphony)
                {
                    return;
                }
                
                var source = PlayClipAsNewSource(clip, loop);
                source.transform.SetParent(transform);
                source.loop = loop;
                source.volume = Mathf.Clamp01(volume);
                _playingTrackSources[id] = source;
            }
        }
        
        public void PlayTrack(string id, bool loop)
        {
            if (_trackClipsDict.TryGetValue(id, out var clip))
            {
                if (_playingTrackSources.ContainsKey(id) && !allowTrackPolyphony)
                {
                    return;
                }
                
                var source = PlayClipAsNewSource(clip, loop);
                source.transform.SetParent(transform);
                source.volume = musicVolume;
                _playingTrackSources[id] = source;
            }
        }

        public void StopTrack(string id)
        {
            if (_playingTrackSources.TryGetValue(id, out var source))
            {
                _playingTrackSources.Remove(id);
                if (source != null)
                {
                    Destroy(source.gameObject);
                }
            }
        }
        public AudioSource PlayClipAsNewSource(AudioClip clip, bool loop = false)
        {
            var source = InstantiateAudioSource();
            source.clip = clip;
            source.spatialBlend = 0f;
            source.volume = SoundVolume;
            if (loop)
            {
                source.loop = true;
            }
            else
            {
                Timing.RunCoroutine(_DestroyAfterDuration(source, source.clip.length));
            }
            source.Play();
            return source;
        }

        public AudioSource PlayClipAsNewSource3D(AudioClip clip, Vector3 position, bool loop = false)
        {
            var source = InstantiateAudioSource();
            source.transform.position = position;
            source.clip = clip;
            source.spatialBlend = 1f;
            if (loop)
            {
                source.loop = true;
            }
            else
            {
                Timing.RunCoroutine(_DestroyAfterDuration(source, source.clip.length));
            }
            source.Play();
            return source;
        }

        public void SetMusicVolume(float value)
        {
            foreach (AudioSource source in _playingTrackSources.Values)
            {
                source.volume = Mathf.Clamp01(value);
            }
        }

        public void SetSoundVolume(float value)
        {
            foreach (AudioSource source in _playingAudioSources)
            {
                source.volume = Mathf.Clamp01(value);
            }
        }

        // public struct LoadAudioAssetContext
        // {
        //     public AudioClip AudioClip { get; set; }
        // }

        // public delegate void OnLoadAudioAssetCompleted(LoadAudioAssetContext context);

        // public void LoadAudioAsset(AssetReference assetReference, OnLoadAudioAssetCompleted completed = null)
        // {
        //     Addressables.LoadAssetAsync<AudioClip>(assetReference).Completed += (a) =>
        //     {
        //         if (a.Status == AsyncOperationStatus.Succeeded)
        //         {
        //             completed?.Invoke(new()
        //             {
        //                 AudioClip = a.Result,
        //             });
        //         }
        //     };
        // }

        public async void LoadAudioAssets(AudioAssetsData data)
        {
            if (data == null || data.AudioClips == null) return;
            
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

        AudioSource InstantiateAudioSource()
        {
            return new GameObject("Audio Source Instance", typeof(AudioSource)).GetComponent<AudioSource>();
        }

        IEnumerator<float> _DestroyAfterDuration(AudioSource source, float duration)
        {
            yield return Timing.WaitForSeconds(duration);

            _playingAudioSources.Remove(source);
            if (source != null) Destroy(source.gameObject);
        }

        #endregion
    }
}
