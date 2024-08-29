using System;
using UnityEngine;

namespace UZSG.FPP
{
    [Serializable]
    public struct BobSettings
    {
        public const float AmplitudeFactor = 0.005f;
        public const float FrequencyFactor = 20;
        public const float RecoveryFactor = 2f;

        public float Amplitude;
        public float Frequency;
        public float Recovery;
        public bool MaintainForwardLook;
        public float LookDistance;

        [Space]
        [Header("Transform Run Bob"), Tooltip("Bobbing animation of the gun while running.")]
        public float TransformDamping;
        /// <summary>
        /// The position and rotation of the viewmodel when running.
        /// Typically a "low ready side" position. idk
        /// </summary>
        public Vector3 RunningPosition;
        /// <summary>
        /// The position and rotation of the viewmodel when running.
        /// Typically a "low ready side" position. idk
        /// </summary>
        public Vector3 RunningRotation;
    }
}