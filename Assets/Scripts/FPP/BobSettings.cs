using System;
using UnityEngine;

namespace UZSG.FPP
{
    [Serializable]
    public struct BobSettings
    {
        /// <summary>
        /// Constant balancing multiplier for Amplitude.
        /// </summary>
        public const float AmplitudeFactor = 0.005f;
        /// <summary>
        /// Constant balancing multiplier for Frequency.
        /// </summary>
        public const float FrequencyFactor = 20;
        /// <summary>
        /// Constant balancing multiplier for Recovery.
        /// </summary>
        public const float RecoveryFactor = 2f;

        public string Id;
        public float Threshold;
        public float Amplitude;
        public float Frequency;
        public float Recovery;
        public bool MaintainForwardLook;
        public float LookDistance;
    }
}