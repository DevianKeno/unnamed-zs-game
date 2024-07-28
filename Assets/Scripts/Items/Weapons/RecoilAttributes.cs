using System;
using UnityEngine;

namespace UZSG.Items.Weapons
{
    public enum RecoilType {
        Additive, Recovered
    }

    [Serializable]
    public struct RecoilAttributes
    {
        public RecoilType Type;
        /// Vertical Recoil
        public float VerticalRecoilAmount;
        public float VerticalRecoilRandomness;

        /// Horizontal Recoil
        public float HorizontalRecoilAmount;
        public float HorizontalRecoilRandomness;
        [Range(-1, 1)]
        public float HorizontalRecoilDirection; /// Positive for right, negative for left

        /// Recoil
        public float Speed;
        public float Duration;
        public float RecoverySpeed;
        public float RecoveryDuration;

#region Unused
        // /// Recoil Pattern
        // public enum RecoilPatternType { StraightUp, Zigzag, Random }
        // public RecoilPatternType PatternType;
        // public Vector2[] PatternData; // List of points in the pattern

        // /// Recoil Spread
        // public float SpreadIncreasePerShot;
        // public float MaxSpread;

        // /// Recoil Control
        // public float ControlModifier;

        // /// Camera Shake
        // public float ShakeIntensity;
        // public float ShakeDuration;
#endregion
    }
}