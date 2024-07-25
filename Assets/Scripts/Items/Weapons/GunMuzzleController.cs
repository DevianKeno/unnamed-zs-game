using System.Collections;
using UnityEngine;

namespace UZSG.Systems
{
    public class GunMuzzleController : MonoBehaviour
    {
        public bool EnableMuzzleFlash = true;
        public float FlashIntensity = 1f;
        public Material FlashMaterial;
        public bool EnableSmoke = true;
        public Material SmokeMaterial;

        [SerializeField] ParticleSystem muzzleFlashParticle;
        [SerializeField] ParticleSystem muzzleSmokeParticle;
        [SerializeField] Light muzzleFlashLight;

        void Start()
        {
            if (FlashMaterial != null) muzzleFlashParticle.GetComponent<ParticleSystemRenderer>().material = FlashMaterial;
            if (SmokeMaterial != null) muzzleSmokeParticle.GetComponent<ParticleSystemRenderer>().material = SmokeMaterial;
        }

        public void Fire()
        {
            if (EnableMuzzleFlash)
            {
                muzzleFlashParticle?.Play();
                LeanTween.value(gameObject, FlashIntensity, 0, 0.1f)
                .setOnUpdate((float i) => 
                {
                    muzzleFlashLight.intensity = i;
                });
            }

            if (EnableSmoke)
            {
                muzzleSmokeParticle?.Play();
            }
        }
    }
}