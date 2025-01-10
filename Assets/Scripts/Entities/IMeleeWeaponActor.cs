using UnityEngine;

namespace UZSG.Entities
{
    public interface IMeleeWeaponActor
    {
        public GameObject gameObject { get; }
        /// <summary>
        /// Entity's forward direction.
        /// </summary>
        public Vector3 Forward { get; }
        /// <summary>
        /// Entity's right direction.
        /// </summary>
        public Vector3 Right { get; }
        /// <summary>
        /// Entity's upward direction.
        /// </summary>
        public Vector3 Up { get; }
        /// <summary>
        /// World space position of the entity's eye level.
        /// </summary>
        public Vector3 EyeLevel { get; }
    }
}