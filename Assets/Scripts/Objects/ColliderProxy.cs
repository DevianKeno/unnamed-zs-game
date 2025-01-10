using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Data;

namespace UZSG.Objects
{
    /// <summary>
    /// Serves as a midman for sending event calls to other scripts from a script with a collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ColliderProxy : MonoBehaviour
    {
        public event Action<Collider> OnTriggerEntered;
        public event Action<Collider> OnTriggerStayed;
        public event Action<Collider> OnTriggerExited;

        void OnTriggerEnter(Collider other)
        {
            OnTriggerEntered?.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            OnTriggerStayed?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            OnTriggerExited?.Invoke(other);
        }
    }
}