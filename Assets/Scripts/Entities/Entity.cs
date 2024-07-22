using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] protected EntityData entityData;
        public EntityData EntityData => entityData;
        public virtual void OnSpawn() { }
    }
}