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

    VehicleEntity vehicleEntity;
    VehicleController vehicleController;


    // Start is called before the first frame update
    void Start()
    {
        vehicleEntity = GetComponent<VehicleEntity>();
        vehicleController = GetComponent<VehicleController>();
        wheelColliders = vehicleEntity.FrontVehicleWheels.Concat(vehicleEntity.RearVehicleWheels).ToList();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < 2; i++)
        {
            wheelColliders[i].GetWorldPose(out WheelPosition, out WheelRotation);
            wheelMeshes[i].transform.position = WheelPosition;
            wheelMeshes[i].transform.rotation = WheelRotation;
        }
    }
}
