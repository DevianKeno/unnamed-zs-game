using System;

using UnityEngine;

using UZSG.Worlds;

namespace UZSG.Systems
{
    public class WorldManager : MonoBehaviour, IInitializeable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// The currently loaded world.
        /// </summary>
        public World CurrentWorld { get; set; }

        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneInit;

        internal void Initialize()
        {
            Game.Console.Log("Initializing World...");

            OnDoneInit?.Invoke();
        }

        public void CreateWorld(string name)
        {
            
        }

        public void LoadLevel(string name)
        {
            // Game.Entity.Spawn("player", new (0f, 1f, 0f));
        }
    }
}