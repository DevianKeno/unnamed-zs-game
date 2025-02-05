using UnityEngine;

using UZSG.Entities;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents entities that are detectable by the Player (or the other way around?)
    /// </summary>
    public interface IPlayerDetectable
    {
        public bool IsAlive { get; }
        /// <summary>
        /// The range of which this entity can see players.
        /// </summary>
        public float PlayerVisionRange { get; }
        public Transform transform { get; }
        public Vector3 Position { get; }

        /// <summary>
        /// Notify this entity that the player has detected it.
        /// </summary>
        public void NotifyDetection(Player player);
    }
}