using System;
using UnityEngine;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        public abstract EntityData Data { get; }
        public abstract void OnSpawn();
    }
}