using UnityEngine;
using TMPro;

namespace UZSG.Entities
{
    /// <summary>
    /// Laggy af
    /// </summary>
    public class DamageText : Entity
    {
        [SerializeField] string damage;
        public string Damage
        {
            get
            {
                return text.text;
            }
            set
            {
                text.text = value;
            }
        }
        public Color Color
        {
            get
            {
                return text.color;
            }
            set
            {
                text.color = value;
            }
        }
        public float Force;

        [Space]
        [SerializeField] TextMeshPro text;
        [SerializeField] Rigidbody rb;

        void OnValidate()
        {
            if (Application.isPlaying) return;

            Damage = damage;
        }

        public override void OnSpawn()
        {
            ShootUp(Vector3.up, Force);
            Destroy(gameObject, 3f);
        }
        
        public void ShootUp(Vector3 impactDirection, float force = 5)
        {
            Vector3 direction = impactDirection + Vector3.up;
            rb.AddForce(force * direction.normalized, ForceMode.Impulse);
        }
    }
}