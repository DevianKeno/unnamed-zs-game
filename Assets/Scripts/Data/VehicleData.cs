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
    public enum VehicleType { NonFuel, Fuel }

    [Serializable]
    [CreateAssetMenu(fileName = "New Vehicle Data", menuName = "UZSG/Vehicle Data")]
    public class VehicleData : BaseData
    {
        [Header("Vehicle Data")]
        public string Name;
        [TextArea] public string Description;
        public AssetReference Model;
        public VehicleType Type;
        public float FuelConsumption;

        [Range(20, 250)]
        public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
        [Range(10, 120)]
        public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
        [Space(20)]
        public AnimationCurve powerCurve;   // Experimental Power/Torque Curve for more customized acceleration
        public bool frontPower = true;   // Send Power to Front Wheels
        public bool rearPower = true;   // Send Power to Rear Wheels
        [Space(20)]
        [Range(10, 45)]
        public float maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
        [Range(0.1f, 1f)]
        public float steeringSpeed = 0.5f; // How fast the steering wheel turns.
        [Space(20)]
        [Range(100, 600)]
        public int brakeForce = 350; // The strength of the wheel brakes.
        [Range(1, 10)]
        public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.
        public float antiRoll = 5000f; // How strong the anti roll bar is

        [Header("Vehicle Attributes")]
        public List<AttributeData> Attributes;
    }
}
