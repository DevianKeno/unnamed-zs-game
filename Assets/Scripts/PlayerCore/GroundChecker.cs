using UnityEngine;

namespace UZSG.Players
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] bool _isGrounded;
        public bool IsGrounded => _isGrounded;

        [SerializeField] BoxCollider boxCollider;

        public void OnTriggerEnter(Collider other)
        {
            if (_isGrounded = other.CompareTag("Ground"))
            {
                _isGrounded = true;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (_isGrounded = other.CompareTag("Ground"))
            {
                _isGrounded = false;
            }
        }
    }
}