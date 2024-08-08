using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Data;

namespace UZSG.Systems
{
    /// <summary>
    /// Audio sources pool.
    /// </summary>
    public class AudioSourceController : MonoBehaviour
    {
        public AudioAssetsData AudioAssetsData;

        bool _hasAudioPool;
        Dictionary<string, AudioClip> audioClips = new();
        public List<AudioClip> AudioClips => audioClips.Values.ToList();
        Queue<AudioSource> availableSources = new();

        public void CreateAudioPool(int size)
        {
            if (_hasAudioPool) return;
            _hasAudioPool = true;
            
            var parent = new GameObject("Audio Source Pool");
            parent.transform.SetParent(transform);
            for (int i = 0; i < size; i++)
            {
                var go = new GameObject($"Audio Source ({i})");
                var audioSource = go.AddComponent<AudioSource>();
                go.transform.parent = parent.transform;
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
                Game.Console.LogAndUnityLog($"AudioAssetId reference not set. No audio will play.");
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

        public void PlaySound(string name)
        {
            if (availableSources.Count > 0)
            {
                if (audioClips.ContainsKey(name))
                {
                    var source = availableSources.Dequeue();
                    source.clip = audioClips[name];
                    source.Play();
                    StartCoroutine(ReturnToFootstepPoolWhenFinished(source));
                };
            }
            /// else maybe still try to play sound, exceeding the pool size
            /// but delete the extra source after.
            /// Can also add a flag whether to allow to exceed pool size
        }

        IEnumerator ReturnToPoolWhenFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            availableSources.Enqueue(source);
        }

        IEnumerator ReturnToFootstepPoolWhenFinished(AudioSource source)
        {
            yield return new WaitForSeconds(source.clip.length);
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