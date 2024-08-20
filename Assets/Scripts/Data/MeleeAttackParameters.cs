using System;
using UnityEngine;

namespace UZSG.Data
{
    public enum MeleeSwingType {
        Raycast, Swingcast
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Melee Attack Parameters", menuName = "UZSG/Attacks/Melee Attack")]
    public class MeleeAttackParameters : BaseData
    {
        public MeleeSwingType SwingType;
        [Space]
        
        public float Delay;
        public float Speed;
        public Vector3 Origin;

        [Header("Swingcast")]
        public int RayCount;
        /// Swingcast
        [Range(0, 360)]
        public float Angle;

    }
}