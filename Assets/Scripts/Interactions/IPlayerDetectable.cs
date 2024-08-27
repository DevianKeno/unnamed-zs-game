using UnityEngine;
using UZSG.Entities;

namespace UZSG.Interactions
{
    /// <summary>
    /// Represents entities that are detectable by the Player (or the other way around?)
    /// </summary>
    public interface IPlayerDetectable
    {
        public Vector3 Position { get; }
        public float PlayerDetectionRadius { get; }

        public void DetectPlayer(Entity etty);
        //public void AttackPlayer(Entity etty); /// paghiwalayin mo to pre detectable nga tapos may attack??
    }
}