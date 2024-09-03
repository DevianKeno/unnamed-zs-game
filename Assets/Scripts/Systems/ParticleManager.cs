using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Systems
{
    public class ParticleManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        Dictionary<string, ParticleData> _particlesDict = new();

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.Log("Reading data: Particles...");
            foreach (var particle in Resources.LoadAll<ParticleData>("Data/Particles"))
            {
                _particlesDict[particle.name] = particle;
            }
        }

        /// <summary>
        /// Spawn particle instance at position.
        /// </summary>
        public void Spawn(string name, Vector3 position)
        {
            if (_particlesDict.TryGetValue(name, out var particle))
            {
                Addressables.LoadAssetAsync<GameObject>(particle.Asset).Completed += (a) =>
                {
                    if (a.Status == AsyncOperationStatus.Succeeded)
                    {
                        Instantiate(a.Result, position, Quaternion.identity, transform);
                    }
                };
            }
        }
    }
}
