using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralEquipAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float duration = 1.0f;           // Duration of the equip animation
    public Vector3 startOffset = new Vector3(0, -1, -2);  // Start position offset
    public Vector3 endOffset = Vector3.zero;             // End position offset (typically zero)

    [Header("Rotation Settings")]
    public Vector3 startRotation = new Vector3(45, 0, 0); // Start rotation offset (in degrees)
    public Vector3 endRotation = Vector3.zero;            // End rotation offset (typically zero)

    [Header("Smoothing")]
    public AnimationCurve positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // Position interpolation curve
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // Rotation interpolation curve

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float elapsedTime;
    private bool isAnimating = false;

    void Start()
    {
        // Record the initial transform of the viewmodel
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        // Start the equip animation
        StartEquipAnimation();
    }

    void Update()
    {
        if (isAnimating)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Interpolate position and rotation
            Vector3 position = Vector3.Lerp(startOffset, endOffset, positionCurve.Evaluate(t));
            Vector3 rotation = Vector3.Lerp(startRotation, endRotation, rotationCurve.Evaluate(t));

            // Apply the interpolated position and rotation
            transform.localPosition = initialPosition + position;
            transform.localRotation = initialRotation * Quaternion.Euler(rotation);

            // End the animation after the duration is over
            if (t >= 1.0f)
            {
                isAnimating = false;
            }
        }
    }

    public void StartEquipAnimation()
    {
        // Reset the timer and start the animation
        elapsedTime = 0f;
        isAnimating = true;
    }
}

