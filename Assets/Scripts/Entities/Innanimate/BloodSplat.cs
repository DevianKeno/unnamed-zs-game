using UnityEngine;

namespace UZSG.Entities
{
    public class BloodSplat : Entity
    {
        [SerializeField] ParticleSystem bloodSplat;
        [SerializeField] ParticleSystem bloodCloud;

        public override void OnSpawn()
        {
            bloodSplat.Play();
            bloodCloud.Play();
            Destroy(gameObject, 3f);
        }
    }
}