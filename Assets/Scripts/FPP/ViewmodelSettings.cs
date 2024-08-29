using System;
using UnityEngine;

namespace UZSG.FPP
{
    [Serializable]
    public struct ViewmodelSettings
    {
        public bool UseOffsets;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        [Header("Bob Settings")]
        public Vector3 RunBobPosition;
        public Vector3 RunBobRotation;
    }
}
