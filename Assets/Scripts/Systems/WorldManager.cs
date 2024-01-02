using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UZSG.Systems
{
    public class WorldManager : MonoBehaviour, IInitializable
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// The current loaded world.
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Called after the World has been successfully initialized.
        /// </summary>
        public event Action OnDoneInit;

        internal void Initialize()
        {
            Game.Console?.Log("Initializing world...");

            OnDoneInit?.Invoke();
        }

        public void CreateWorld(string name)
        {
            
        }

        public void LoadWorld(string name)
        {
            // Game.Entity.Spawn("player", new (0f, 1f, 0f));
        }
    }
}