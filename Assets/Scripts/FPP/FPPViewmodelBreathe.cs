using UnityEngine;

namespace UZSG.FPP
{
    public class FPPViewmodelBreathe : MonoBehaviour
    {
        [SerializeField] Transform viewmodelHolder;
        
        [Header("Breathing Settings")]
        public bool Enabled = true;
        [Tooltip("How fast the breathing cycle is.")]
        public float BreatheSpeed = 1f;
        [Tooltip("The amount of breathing movement.")]
        public float BreatheAmount = 0.05f;

        Vector3 initialPosition;
        float breatheTimer;

        void Start()
        {
            initialPosition = viewmodelHolder.localPosition;
        }

        void Update()
        {
            if (!Enabled) return;

            breatheTimer += Time.deltaTime * BreatheSpeed;
            float offset = Mathf.Sin(breatheTimer) * BreatheAmount;
            viewmodelHolder.localPosition = initialPosition + new Vector3(0, offset, 0);
        }
    }
}
