using System.Collections;
using UnityEngine;

namespace UZSG.Systems
{
    public class GunMuzzleController : MonoBehaviour
    {
        [Header("Muzzle Flash Settings")]
        public bool EnableMuzzleFlash = true;
        public Material FlashMaterial;

        [Space(10)]
        [Header("Smoke Settings")]
        public bool EnableSmoke = true;
        public Material SmokeMaterial;

        [field: Space(10)]
        [field: Header("Flash Settings")]
        public bool EnableFlash = true;
        public float FlashIntensity = 1f;
        public float FlashDuration = 1f;
        [Space(5)]
        public bool WorldFlash = true;
        public float WorldFlashIntensity = 1f;

        [Space(10)]
        [SerializeField] ParticleSystem muzzleFlashParticle;
        [SerializeField] ParticleSystem muzzleSmokeParticle;
        [SerializeField] Light viewmodelLight;
        [SerializeField] Light worldLight;

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
            }

            if (EnableSmoke)
            {
                muzzleSmokeParticle?.Play();
            }

            if (EnableFlash)
            {
                LeanTween.value(gameObject, 1, 0, FlashDuration)
                .setOnUpdate((float i) => 
                {
                    viewmodelLight.intensity = i * FlashIntensity;
                    if (WorldFlash) 
                    {
                        worldLight.intensity = i * WorldFlashIntensity;
                    }
                });
            }
        }
    }
}