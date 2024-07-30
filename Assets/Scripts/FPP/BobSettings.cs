using System;

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
    }
}