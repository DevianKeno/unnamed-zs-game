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
        bool _hasAudioPool;

        Dictionary<string, AudioClip> audioClips = new();
        public List<AudioClip> AudioClips => audioClips.Values.ToList();
        Queue<AudioSource> availableSources = new();

        public void CreateAudioPool(int size)
        {
            if (_hasAudioPool) return;
            _hasAudioPool = true;
            
            var parent = new GameObject("Audio Source Pool");
            parent.transform.parent = transform;
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

        int CalculateOptimalAudioPoolSize(int clipSize, float roundsPerMinute, float maxAudioDurationSeconds)
        {
            float roundsPerSecond = roundsPerMinute / 60f;
            float maxShotsWithinAudioClipDuration = roundsPerSecond * maxAudioDurationSeconds;
            int optimalPoolSize = Mathf.CeilToInt(maxShotsWithinAudioClipDuration);
            optimalPoolSize = Mathf.Min(optimalPoolSize, 30);

            return optimalPoolSize;
        }

        public async void LoadAudioAssetIds(EquipmentAudioData data, Action onCompleted = null)
        {
            var loadAudioTask = Game.Audio.LoadAudioAssets(data.AudioAssetIds, (result) =>
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
                    StartCoroutine(ReturnToPoolWhenFinished(source));
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
    }
}