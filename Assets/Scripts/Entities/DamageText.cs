using UnityEngine;
using TMPro;

namespace UZSG.Entities
{
    /// <summary>
    /// Laggy af
    /// </summary>
    public class DamageText : Entity
    {
        public string Text
        {
            get
            {
                return textTMP.text;
            }
            set
            {
                textTMP.text = value;
            }
        }
        public Color Color
        {
            get
            {
                return textTMP.color;
            }
            set
            {
                textTMP.color = value;
            }
        }
        public float Force;

        [Space]
        [SerializeField] TextMeshPro textTMP;
        [SerializeField] Rigidbody rb;

        public override void OnSpawnEvent()
        {
            ShootUp(Vector3.up, Force);
            Destroy(gameObject, 2f);
        }
        
        public void ShootUp(Vector3 impactDirection, float force = 5)
        {
            Vector3 direction = impactDirection + Vector3.up;
            rb.AddForce(force * direction.normalized, ForceMode.Impulse);
        }
    }
}