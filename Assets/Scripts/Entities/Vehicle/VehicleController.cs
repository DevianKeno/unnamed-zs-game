using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.Systems;

public class VehicleController : MonoBehaviour
{
    /// <summary>
    /// This vehicle controller is only for testing, I am not entirely sure how to perfectly implement this.
    /// </summary>
    [SerializeField] VehicleEntity vehicle;
    Vector3 vehicleForward;
    InputAction forward;
    bool isPressingForward;

    // Vehicle Speed Handler
    float currentSpeed;
    float turnSpeed = 100f;

    private void Awake()
    {
        vehicle = this.GetComponent<VehicleEntity>();
    }

    private void Start()
    {
        vehicleForward = this.transform.forward;
        forward = Game.Main.GetInputAction("Move", "Player Move");
        forward.started += OnMoveInput;
        forward.canceled += OnMoveInput;
    }

    private void OnMoveInput(InputAction.CallbackContext obj)
    {
        if (obj.started)
        {
            isPressingForward = true;
        }
        else
        {
            isPressingForward = false;
        }
        Debug.Log(isPressingForward);
    }

    // Update is called once per frame
    void Update()
    {
        if(vehicle.Driver != null)
        {
            vehicle.Driver.transform.localPosition = new Vector3(0, -1, 1);

            // Handle forward movement
            if (Input.GetKey(KeyCode.W))
            {
                // Accelerate forward
                currentSpeed = Mathf.Min(currentSpeed + vehicle.Vehicle.AccelerationRate * Time.deltaTime, vehicle.Vehicle.MaxSpeed);
                vehicleForward = vehicle.transform.forward; // Update vehicleForward based on current rotation
                vehicle.transform.position += vehicleForward * currentSpeed * Time.deltaTime;

                // Handle turning
                float turnInput = 0;
                if (Input.GetKey(KeyCode.A))
                {
                    turnInput = -1; // Turn left
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    turnInput = 1; // Turn right
                }

                if (turnInput != 0)
                {
                    // Rotate the vehicle around its Y-axis based on the input
                    vehicle.transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Decelerate (reverse) if moving forward
                currentSpeed = Mathf.Min(currentSpeed - vehicle.Vehicle.AccelerationRate * Time.deltaTime, vehicle.Vehicle.MaxSpeed);
                vehicleForward = vehicle.transform.forward; // Update vehicleForward based on current rotation
                vehicle.transform.position -= vehicleForward * currentSpeed * Time.deltaTime;
            }
            else
            {
                // Decelerate to stop when no key is pressed
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0, vehicle.Vehicle.AccelerationRate * Time.deltaTime);
            }
        }
    }
}
