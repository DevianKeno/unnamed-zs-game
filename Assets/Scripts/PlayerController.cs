using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZS
{
    public class PlayerController : MonoBehaviour
    {
        public float playerSpeed;
        PlayerInput playerInput;
        InputAction move;
        InputAction jump;
        Vector3 frameVelocity;
        bool isGrounded;
        public bool IsGrounded { get => isGrounded; }

        #region Components

        [SerializeField] private Player player;
        [SerializeField] private Rigidbody rb;

        #endregion

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            move = playerInput.actions.FindAction("Move");
            jump = playerInput.actions.FindAction("Jump");
        }

        void OnEnable()
        {
            player.Attributes.MovementSpeed.OnValueChanged += GetSpeedChange;
            playerSpeed = player.Attributes["MovementSpeed"];
            move.Enable();
        }

        void GetSpeedChange(Attribute attribute)
        {
            playerSpeed = attribute.Value;
        }

        void OnDisable()
        {
            move.Disable();
        }

        void Update()
        {

        }

        Vector2 movement;

        void FixedUpdate()
        {
            HandleMovement();
            HandleJump();

            ApplyMovement();
        }

        void HandleJump()
        {
            //
        }

        void HandleMovement()
        {
            movement = move.ReadValue<Vector2>() * Time.deltaTime;
        }

        void ApplyMovement()
        {
            transform.position += new Vector3(movement.x, 0, movement.y);
        }
    }
}
