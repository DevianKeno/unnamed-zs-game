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

        [SerializeField] SphereCollider sphereCollider;

        void OnValidate()
        {
            var target = transform.position;
            target.z = Distance;
            transform.position = target;
            sphereCollider.radius = Radius;
        }

        void OnTriggerEnter(Collider other)
        {
            OnLookEnter?.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            OnLookStay?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            OnLookExit?.Invoke(other);
        }
    }
}