using System;
using UnityEngine;

using UZSG.Attacks;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Melee Attack Parameters", menuName = "UZSG/Attacks/Melee Attack")]
    public class MeleeAttackParametersData : BaseData
    {
        public CastType SwingType;
        [Space]
        
        public float Range;
        public float Duration;
        public float Delay;
        public LayerMask Layer;

        [Header("Swingcast")]
        public int RayCount;
        [Range(0, 360)]
        public float AngleWidth;
        public Vector3 RotationOffset;
        public bool Flip;

        [Header("Debugging")]
        public bool Visualize;
        public bool IncludeRotationOffset = true;
    }
}