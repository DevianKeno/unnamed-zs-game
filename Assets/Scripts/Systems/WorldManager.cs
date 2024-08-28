using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Saves;
using UZSG.Worlds;

namespace UZSG.Systems
{
    public struct CreateWorldOptions
    {
        public string Name;
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
    }

    public class WorldManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        [SerializeField] World currentWorld;
        /// <summary>
        /// The currently loaded world.
        /// </summary>
        public World CurrentWorld => currentWorld;
        public bool HasWorld { get; private set; } = false;

        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneInit;

        void Awake()
        {
            if (currentWorld != null) HasWorld = true;
        }

        internal void Initialize()
        {
            Game.Console.Log("Initializing World...");

            RetrieveSavedWorlds();

            currentWorld.Initialize();///testing

            OnDoneInit?.Invoke();
        }

        void RetrieveSavedWorlds()
        {
            
        }

        public void CreateWorld(CreateWorldOptions options)
        {
            options.CreatedDate = DateTime.Now;
        }

        public void LoadLevel(string name)
        {
            /// assuming we're already in the loading screen
        }

        async void LoadLevelAsync(LevelData data)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(data.LevelAsset);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var level = Instantiate(handle.Result, transform);
                var world = level.GetComponent<World>();
                world.Initialize();
            }
            else
            {
                Game.Console.LogError($"Failed to load level '{data.Id}`");
                /// go back to Title Screen
            }
        }
    }

    public class LevelData
    {
        public string Id;
        public AssetReference LevelAsset;
    }
}