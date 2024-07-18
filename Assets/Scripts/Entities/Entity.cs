using System;
using UnityEngine;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        public abstract EntityData EntityData { get; }
        public EntityData Data => EntityData;
        public abstract void OnSpawn();
    }
}