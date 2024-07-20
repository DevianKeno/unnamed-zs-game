using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UZSG.Items.Weapons;

namespace UZSG.Systems
{
    public class AudioSourceController : MonoBehaviour
    {
        [SerializeField] int poolSize = 8;
        public int PoolSize
        {
            get { return poolSize; }
            set { poolSize = value; }
        }
        Dictionary<string, AudioClip> audioClips = new();
        Queue<AudioSource> availableSources = new();

        void Start()
        {
            var parent = new GameObject("Audio Source Pool");
            parent.transform.parent = transform;
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("")
                {
                    name = $"Audio Source ({i})"
                };
                var audioSource = go.AddComponent<AudioSource>();
                go.transform.parent = parent.transform;
                audioSource.playOnAwake = false;
                availableSources.Enqueue(audioSource);
            }
        }

        public void LoadAudioAssetIds(List<AudioAssetId> audioAssetIdlist)
        {
            foreach (var item in audioAssetIdlist)
            {
                Game.Audio.LoadAudioAsset(item.AudioAsset, (result) =>
                {
                    audioClips[item.Id] = result.AudioClip;
                });
            }
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
            /// else still try to play sound, exceeding the pool size
            /// but delete the extra source after.
            /// Can also add a flag whether to allow overflows
        }

        IEnumerator ReturnToPoolWhenFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            availableSources.Enqueue(source);
        }
    }
}