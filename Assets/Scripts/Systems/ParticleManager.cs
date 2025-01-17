using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Systems
{
    public class ParticleManager : MonoBehaviour, IInitializeable
    {
        static readonly string[] POOLED_PARTICLES = { "material_break" };
        const int MAX_POOL_SIZE = 512;

        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ParticleData> _particlesDict = new();

        Dictionary<ParticleData, Queue<Particle>> _particlePoolDict; 

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.LogInfo("Reading data: Particles...");
            foreach (var particle in Resources.LoadAll<ParticleData>("Data/Particles"))
            {
                _particlesDict[particle.name] = particle;
            }

            _particlePoolDict = new();
            foreach (string id in POOLED_PARTICLES)
            {
                CreateParticlePool<MaterialBreak>(id, size: 8);
            }
        }

        void CreateParticlePool<T>(string id, int size) where T : Particle
        {
            if (!_particlesDict.TryGetValue(id, out var particleData)) return;

            _particlePoolDict[particleData] = new Queue<Particle>();
            Addressables.LoadAssetAsync<GameObject>(particleData.Asset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    size = Math.Clamp(size, 0, MAX_POOL_SIZE);
                    for (int i = 0; i < size; i++)
                    {
                        var go = Instantiate(a.Result, transform);
                        if (go.TryGetComponent(out Particle particle))
                        {
                            particle.ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                            var p = particle.ParticleSystem.main;
                            p.stopAction = ParticleSystemStopAction.None;
                            _particlePoolDict[particleData].Enqueue(particle);
                        }
                    }
                }
            };
        }


        #region Public methods
        /// <summary>
        /// Spawn particle instance at position.
        /// </summary>
        public void Create(string name, Vector3 position)
        {
            if (_particlesDict.TryGetValue(name, out var particleData))
            {
                Create(particleData, position);
            }
        }

        /// <summary>
        /// Spawn particle instance at position.
        /// </summary>
        public void Create(ParticleData particleData, Vector3 position)
        {
            Addressables.LoadAssetAsync<GameObject>(particleData.Asset).Completed += (a) =>
            {
                if (a.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                }
            };
        }

        public void Create<T>(string id, Vector3 position, Action<T> onSpawn = null) where T : Particle
        {
            if (_particlesDict.TryGetValue(id, out var particleData))
            {
                Create<T>(particleData, position, onSpawn);
            }
        }

        public void Create<T>(ParticleData particleData, Vector3 position, Action<T> onSpawn = null) where T : Particle
        {
            if (_particlePoolDict.ContainsKey(particleData))
            {
                var particle = _particlePoolDict[particleData].Dequeue();
                particle.gameObject.transform.position = position;
                particle.ParticleSystem.Play();
                _particlePoolDict[particleData].Enqueue(particle);
            }
            else
            {
                Addressables.LoadAssetAsync<GameObject>(particleData.Asset).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        var go = Instantiate(a.Result, position, Quaternion.identity, transform);
                        if (go.TryGetComponent(out Particle particle))
                        {
                            onSpawn?.Invoke(particle as T);
                        }
                    }
                };
            }
        }

        public ParticleData GetParticleData(string name)
        {
            if (_particlesDict.TryGetValue(name, out ParticleData particleData)){}
            return particleData;
        }

        #endregion
    }
}
