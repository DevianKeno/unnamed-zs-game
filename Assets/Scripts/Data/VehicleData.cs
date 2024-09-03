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

        [Space(20)]
        [Header("Vehicle Fuel Consumption")]
        // Fuel Consumption is modeled based on vehicle current power and speed 
        public float fuelCapacity = 50;
        public float fuelConsumptionPerPower = 0.0005f;
        public float fuelCapacityPerSpeed = 0.002f;
        public float fuelEfficiencyMultiplier = 1;

        [Space(20)]
        [Header("Vehicle Power Settings")]
        [Range(20, 250)] public int maxSpeed = 90; //The maximum speed that the car can reach in km/h.
        [Range(10, 120)] public int maxReverseSpeed = 45; //The maximum speed that the car can reach while going on reverse in km/h.
        public AnimationCurve powerCurve;   // Experimental Power/Torque Curve for more customized acceleration
        public bool frontPower = true;   // Send Power to Front Wheels
        public bool rearPower = true;   // Send Power to Rear Wheels

        [Space(20)]
        [Header("Vehicle Steer Settings")]
        public float turnRadius = 12f; // turn radius of the vehicle, you can refer to real life spec or just make yourself up, ideal is between 10 - 12
        [Range(0.1f, 1f)] public float steeringSpeed = 0.5f; // How fast the steering wheel turns.

        [Space(20)]
        [Header("Vehicle Brake Settings")]
        [Range(100, 1000)] public int brakeForce = 350; // The strength of the wheel brakes.
        [Range(1, 10)] public int decelerationMultiplier = 2; // How fast the car decelerates when the user is not using the throttle.

        [Space(20)]
        [Header("Vehicle Stability Settings")]
        public float antiRoll = 5000f; // How strong the anti roll bar is
        public float steerLimitSpeedThreshold = 90f; // when steer limiting will activate
        [Range(0, 1)] public float steerLimitAmount = 0.5f; // how much will be reduced to the steer axis


        [Header("Vehicle Attributes")]
        public List<AttributeData> Attributes;
    }
}
