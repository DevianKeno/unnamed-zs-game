using System.Collections;
using UnityEngine;

namespace UZSG.Systems
{
    public class GunMuzzleController : MonoBehaviour
    {
        public float FlashIntensity = 1f;
        public Material FlashMaterial;
        public Material SmokeMaterial;

        [SerializeField] ParticleSystem muzzleFlashParticle;
        [SerializeField] ParticleSystem muzzleSmokeParticle;
        [SerializeField] Light muzzleFlashLight;

        void Start()
        {
            if (FlashMaterial != null) muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = FlashMaterial;
            if (SmokeMaterial != null) muzzleSmokeParticle.GetComponent<ParticleSystemRenderer>().material = SmokeMaterial;
        }

        public void OnFire()
        {
            muzzleFlashParticle?.Play();
            muzzleSmokeParticle?.Play();
            LeanTween.value(gameObject, FlashIntensity, 0, 0.1f)
            .setOnUpdate((float i) => 
            {
                muzzleFlashLight.intensity = i;
            });
        }
    }
}