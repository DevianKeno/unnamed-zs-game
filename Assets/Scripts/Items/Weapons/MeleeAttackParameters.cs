using System;
using UnityEngine;
using UZSG.Data;

namespace UZSG.Attacks
{
    public enum CastType {
        Raycast, Swingcast
    }

    [Serializable]
    public struct MeleeAttackParameters
    {
        public CastType CastType { get; set; }
        public float Range { get; set; }
        public float Duration { get; set; }
        public float Delay { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 Direction { get; set; }
        public LayerMask Layer { get; set; }
        public int RayCount { get; set; }
        public float AngleWidth { get; set; }
        /// <summary>
        /// Whether to flip the swing direction.
        /// </summary>
        public bool Flip { get; set; }
        public Vector3 RotationOffset { get; set; }
        public bool Visualize { get; set; }
        public bool IncludeRotationOffset { get; set; }

    }
}