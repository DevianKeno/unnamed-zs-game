using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Crafting;
using UZSG.Data;

namespace UZSG.Data
{
    [Serializable]
    public struct AmmoBaseAttributes
    {
        public float Damage;
        public float Velocity;
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Item Data", menuName = "UZSG/Items/Ammo Data")]
    public class AmmoData : ItemData
    {
        [Header("Ammo Data")]
        public string Caliber;
    }
}