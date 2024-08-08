using System;

using UnityEngine;

using UZSG.Systems;
using UZSG.Data;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        protected const string entityDefaultsPath = "/Resources/Defaults/Entities/";

        [SerializeField] protected EntityData entityData;
        public EntityData EntityData => entityData;
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        [SerializeField] protected AudioSourceController audioSourceController;
        public AudioSourceController AudioSourceController => audioSourceController;

        internal void OnSpawnInternal()
        {
            OnSpawn();
        }

        /// <summary>
        /// Called after the EntityManager spawned callback.
        /// You can modify the entity's attributes before this calls.
        /// </summary>
        public virtual void OnSpawn() { }
    }
}