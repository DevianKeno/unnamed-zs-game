using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPViewmodelSway : MonoBehaviour
    {
        public Player Player;
        [Space]
        
        [Header("Sway Settings")]
        public bool Enabled = true;
        [Tooltip("How smooth the sway transitions are.")]
        [Range(0.1f, 10f)]
        public float Smoothness = 5f;
        [Tooltip("The overall multiplier for sway effect.")]
        [Range(0.1f, 5f)]
        public float Multiplier = 1f;
        [Tooltip("Maximum sway angle horizontally.")]
        [Range(0f, 45f)]
        public float MaxSwayAngleX = 15f;
        [Tooltip("Maximum sway angle vertically.")]
        [Range(0f, 45f)]
        public float MaxSwayAngleY = 10f;
        [Tooltip("Invert the horizontal sway.")]
        public Vector3 RotationOffset = Vector3.zero;
        
        Quaternion _originalRotation;
        InputAction lookInput;

        void Start()
        {
            lookInput = Game.Main.GetInputAction("Look", "Player");
            _originalRotation = transform.localRotation;
        }

        void Update()
        {
            if (!Enabled) return;

            Vector2 deltaMouse = lookInput.ReadValue<Vector2>() * Multiplier;

            deltaMouse.x = Mathf.Clamp(deltaMouse.x, -MaxSwayAngleX, MaxSwayAngleX);
            deltaMouse.y = Mathf.Clamp(deltaMouse.y, -MaxSwayAngleY, MaxSwayAngleY);

            Quaternion rotationX = Quaternion.AngleAxis(-deltaMouse.y, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(deltaMouse.x, Vector3.up);
            Quaternion pivotAdjustment = Quaternion.Euler(RotationOffset);
            Quaternion swayRotation = rotationX * rotationY * pivotAdjustment;

            Quaternion targetRotation = Quaternion.Inverse(transform.localRotation) * swayRotation;

            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Smoothness * Time.deltaTime
            );
        }
    }
}
