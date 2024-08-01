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
            _isGrounded = true;
        }

        public void OnTriggerExit(Collider other)
        {
            _isGrounded = false;
        }
    }
}