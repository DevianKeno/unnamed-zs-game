using System;
using UnityEngine;
using UZSG.Entities;

namespace UZSG.Players
{
    public class PlayerLookRaycaster : MonoBehaviour
    {
        public Player Player;

        public float Distance;
        public float Radius;

        public event Action<Collider> OnLookEnter;
        public event Action<Collider> OnLookStay;
        public event Action<Collider> OnLookExit;

        [SerializeField] CapsuleCollider capsuleCollider;

        void OnValidate()
        {
            var target = transform.position;
            target.z = Distance;
            transform.position = target;
            capsuleCollider.radius = Radius;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == null) return;
            OnLookEnter?.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject == null) return;
            OnLookStay?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == null) return;
            OnLookExit?.Invoke(other);
        }
    }
}