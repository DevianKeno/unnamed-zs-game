using System;
using UnityEngine;

namespace UZSG.Entities
{
    /// <summary>
    /// Represent dynamic objects that appear in the World.
    /// </summary>
    public class Entity : MonoBehaviour, ISpawnable//, IAlive
    {
        public EntityData Data;
        public virtual void Spawn() {}
    }
}