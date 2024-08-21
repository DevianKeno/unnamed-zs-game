using System;
using UnityEngine;
using UZSG.Interactions;

namespace UZSG.Attacks
{
    public class MeleeWeaponCollider : MonoBehaviour
    {
        public LayerMask Layer;
        public event Action<HitboxCollisionInfo> OnCollide;
        public Collider coll;

        public void OnTriggerEnter(Collider other)
        {
            var target = other.GetComponentInParent<ICollisionTarget>();
            if (target != null)
            {
                Bounds thisBounds = coll.bounds;
                Bounds otherBounds = other.bounds;
                Vector3 contactPoint;

                if (Physics.ComputePenetration(
                    coll, thisBounds.center, coll.transform.rotation,
                    other, otherBounds.center, other.transform.rotation,
                    out Vector3 direction, out float distance))
                {
                    // Calculate contact point
                    contactPoint = other.ClosestPointOnBounds(thisBounds.center);
                    Debug.Log("Contact Point: " + contactPoint);
                }
                else
                {
                    // Handle the case where ComputePenetration fails
                    Debug.LogWarning("Penetration computation failed. No contact point available.");
                    // Optionally, calculate a default contact point or handle it differently
                    contactPoint = other.ClosestPointOnBounds(thisBounds.center); // This is a fallback
                }

                // Invoke the collision event with the contact point
                OnCollide?.Invoke(new()
                {
                    Target = target,
                    ContactPoint = contactPoint,
                });
            }
        }
    }
}