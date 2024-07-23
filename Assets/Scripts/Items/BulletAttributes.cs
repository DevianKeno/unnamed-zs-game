using System;
using UnityEngine;

namespace UZSG.Items.Weapons
{
    [Serializable]
    public struct BulletAttributes
    {
        [Tooltip("Bullet entity size. Default is 1.")]
        public float Scale;
        [Tooltip("Bullet travel speed in meters per second.")]
        public float Speed;
        [Tooltip("Spread angle in degrees.")]
        public float Spread;
        [Tooltip("Bullet drop.")]
        public float GravityScale;
        [Tooltip("Minimum distance in meters to render bullet entity.")]
        public float MinRenderDistance;
    }
}