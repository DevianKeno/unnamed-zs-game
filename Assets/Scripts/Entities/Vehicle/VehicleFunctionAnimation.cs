using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZSG.Entities;
using UZSG.Entities.Vehicles;

public class VehicleFunctionAnimation : MonoBehaviour
{
    public List<GameObject> wheelMeshes; // Store all wheel meshes. idk it's better probably to have this private or hidden and just automatically set in script

    [HideInInspector]
    Quaternion WheelRotation;
    [HideInInspector]
    Vector3 WheelPosition;
    [HideInInspector]
    public List<WheelCollider> wheelColliders; // Store all wheel colliders.
    [HideInInspector]
    public Transform steeringWheelTransform;

    VehicleEntity vehicleEntity;
    VehicleController vehicleController;


    // Start is called before the first frame update
    void Start()
    {
        vehicleEntity = GetComponent<VehicleEntity>();
        vehicleController = GetComponent<VehicleController>();
        wheelColliders = vehicleEntity.FrontVehicleWheels.Concat(vehicleEntity.RearVehicleWheels).ToList();

        steeringWheelTransform = this.transform.Find("Steer/Steering Wheel");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AnimateWheelMesh();
        AnimateSteerWheel();
    }

    void AnimateWheelMesh()
    {
        for (int i = 0; i < wheelColliders.Count; i++)
        {
            wheelColliders[i].GetWorldPose(out WheelPosition, out WheelRotation);
            wheelMeshes[i].transform.SetPositionAndRotation(WheelPosition, WheelRotation);
        }
    }

    void AnimateSteerWheel()
    {
        float wheelSteerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, vehicleController.steeringAngle, vehicleController.steeringSpeed);
        steeringWheelTransform.transform.localRotation =  Quaternion.Euler(0, 0, -wheelSteerAngle); // temporary fix since wheel rotation is in negative axis?
    }
}
