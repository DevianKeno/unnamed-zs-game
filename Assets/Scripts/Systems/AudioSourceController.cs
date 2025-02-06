using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Data;

namespace UZSG
{
    /// <summary>
    /// Audio sources pool.
    /// </summary>
    public class AudioSourceController : MonoBehaviour
    {
        public AudioAssetsData AudioAssetsData;

        public bool AllowExceedPool { get; set; } = false;
        bool _hasAudioPool;
        Dictionary<string, AudioClip> audioClips = new();
        public List<AudioClip> AudioClips => audioClips.Values.ToList();
        Queue<AudioSource> availableSources = new();

        GameObject audioSourcePool;

        public void CreateAudioPool(int size)
        {
            if (_hasAudioPool) return;
            _hasAudioPool = true;
            
            audioSourcePool = new GameObject("Audio Source Pool");
            audioSourcePool.transform.SetParent(transform);
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject($"Audio Source ({i})");
                var audioSource = go.AddComponent<AudioSource>();
                go.transform.SetParent(audioSourcePool.transform);
                audioSource.playOnAwake = false;
                availableSources.Enqueue(audioSource);
            }
        }
        
        public void CreateAudioPool(WeaponData data)
        {
            if (_hasAudioPool) return;
            _hasAudioPool = true;
            
            CreateAudioPool(CalculateOptimalAudioPoolSize(
                data.RangedAttributes.ClipSize,
                data.RangedAttributes.RoundsPerMinute,
                2f)
            );
        }

        public async void LoadAudioAssetsData(AudioAssetsData data, Action onCompleted = null)
        {
            if (data == null)
            {
                Game.Console.LogWithUnity($"AudioAssetsData reference not set. No audio will play.");
                return;
            }

            AudioAssetsData = data;
            var loadAudioTask = Game.Audio.LoadAudioAssets(data.AudioClips, (result) =>
            {
                foreach (var audio in result)
                {
                    audioClips[audio.name] = audio;
                }
            });

            await loadAudioTask;
            onCompleted?.Invoke();
        }

        public AudioClip GetClip(string name)
        {
            if (audioClips.TryGetValue(name, out var clip))
            {
                return clip;
            }

            return null;
        }

        public void PlaySound(string name, ulong delaySeconds = 0)
        {
            if (audioClips.TryGetValue(name, out var clip))
            {
                AudioSource source;
                if (availableSources.Count > 0)
                {
                    source = availableSources.Dequeue();
                }
                else if (AllowExceedPool)
                {
                    var go = new GameObject($"Temp Audio Source");
                    go.transform.SetParent(audioSourcePool.transform);
                    source = go.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    Destroy(go, clip.length);
                }
                else
                {
                    return;
                }
                
                source.clip = clip;
                source.Play(delaySeconds);

                if (!AllowExceedPool || availableSources.Contains(source))
                {
                    StartCoroutine(ReturnToPoolWhenFinished(source));
                }
            }
        }
        
        IEnumerator ReturnToPoolWhenFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            availableSources.Enqueue(source);
        }

        int CalculateOptimalAudioPoolSize(int clipSize, float roundsPerMinute, float maxAudioDurationSeconds)
        {
            float roundsPerSecond = roundsPerMinute / 60f;
            float maxShotsWithinAudioClipDuration = roundsPerSecond * maxAudioDurationSeconds;
            int optimalPoolSize = Mathf.CeilToInt(maxShotsWithinAudioClipDuration);
            optimalPoolSize = Mathf.Min(optimalPoolSize, 30);

            return optimalPoolSize;
        }
    }
}