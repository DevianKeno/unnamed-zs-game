using System;
using UnityEngine;
using UZSG.Entities;

namespace UZSG.FPP
{
    public class FPPViewmodelBreathe : MonoBehaviour
    {
        public Player Player;
        [Space]
                
        [Header("Breathing Settings")]
        public bool Enabled = true;
        [Tooltip("How fast the breathing cycle is.")]
        public float BreatheSpeed = 1f;
        [Tooltip("The amount of breathing movement.")]
        public float BreatheAmount = 0.05f;
        
        float breatheTimer;
        Vector3 initialPosition;

        void Start()
        {
            initialPosition = transform.localPosition;
            InitializePlayerEvents();
        }

        void InitializePlayerEvents()
        {
            
        }

        void Update()
        {
            if (!Enabled) return;

            breatheTimer += Time.deltaTime * BreatheSpeed;
            float offset = Mathf.Sin(breatheTimer) * BreatheAmount;
            transform.localPosition = initialPosition + new Vector3(0, offset, 0);
        }
    }
}
