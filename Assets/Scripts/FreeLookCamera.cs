using UnityEngine;
using UZSG.Systems;

namespace UZSG
{
    public class FreeLookCamera : MonoBehaviour
    {
        public bool EnableControls = true;
        public float MoveSpeed = 5f;
        public float Sensitivity = 2f;
        float rotationX = 0f;
        float rotationY = 0f;

        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            Game.UI.OnCursorToggled -= CursorToggledCallback;
        }

        void CursorToggledCallback(bool visible)
        {
            EnableControls = !visible;
        }

        void Update()
        {
            if (EnableControls)
            {
                rotationX += Input.GetAxis("Mouse X") * Sensitivity;
                rotationY -= Input.GetAxis("Mouse Y") * Sensitivity;
                rotationY = Mathf.Clamp(rotationY, -90f, 90f);

                transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
            }

            Vector3 moveDirection = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            moveDirection = MoveSpeed * Time.deltaTime * transform.TransformDirection(moveDirection);
            transform.position += moveDirection;
        }

        public void Initialize()
        {
            Game.UI.OnCursorToggled += CursorToggledCallback;
        }

    }
}
