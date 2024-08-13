using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Attributes;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Data
{
    public enum VehicleType { None, NonFuel, Fuel }

    [Serializable]
    [CreateAssetMenu(fileName = "New Vehicle Data", menuName = "UZSG/Vehicle Data")]
    public class VehicleData : BaseData
    {
        [Header("Vehicle Data")]
        public string Name;
        [TextArea] public string Description;
        public AssetReference Model;
        public VehicleType Type;
        public float AccelerationRate;
        public float MaxSpeed;
        
        [Header("Attributes")]
        public List<AttributeData> Generic;
    }
}
