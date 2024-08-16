using System;

using UnityEngine;

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

        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneInit;

        internal void Initialize()
        {
            Game.Console.Log("Initializing World...");

            RetrieveSavedWorlds();

            // currentWorld.Initialize();///testing

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
            // Game.Entity.Spawn("player", new (0f, 1f, 0f));
        }
    }
}