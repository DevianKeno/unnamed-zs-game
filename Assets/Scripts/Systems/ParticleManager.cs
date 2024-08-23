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
        Dictionary<string, GameObject> _particlesDict = new();

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            var startTime = Time.time;
            Game.Console.Log("Loading Particles...");
            foreach (var particle in Resources.LoadAll<GameObject>("Prefabs/Particles"))
            {
                _particlesDict[particle.name] = particle;
            }
        }

        /// <summary>
        /// Creates an Item object.
        /// </summary>
        public void Spawn(string name, Vector3 position)
        {
            if (_particlesDict.TryGetValue(name, out var particle))
            {
                var go = Instantiate(particle, position, Quaternion.identity, transform);
            }
        }

        // /// <summary>
        // /// Creates an Item object.
        // /// </summary>
        // public void Create(string id, Vector3 position)
        // {
        //     if (_particlesDict.TryGetValue(id, out var particleData))
        //     {
        //         Addressables.LoadAssetAsync<GameObject>(particleData.Asset).Completed += (a) =>
        //         {
        //             if (a.Status == AsyncOperationStatus.Succeeded)
        //             {
        //                 Instantiate(a.Result, position, Quaternion.identity, transform);
        //             }
        //         };
        //     }
        // }
    }
}
