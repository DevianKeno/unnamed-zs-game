using System.Collections;
using UnityEngine;

namespace UZSG.Systems
{
    public class GunMuzzleController : MonoBehaviour
    {
        public float FlashIntensity = 1f;
        [SerializeField] ParticleSystem muzzleFlashParticle;
        [SerializeField] ParticleSystem muzzleSmokeParticle;
        [SerializeField] Light muzzleFlashLight;

        public void OnFire()
        {
            muzzleFlashParticle?.Play();
            // muzzleSmokeParticle?.Play();

            StartCoroutine(StopMuzzleFlashParticles());

            LeanTween.value(gameObject, FlashIntensity, 0, 0.1f)
            .setOnUpdate((float i) => 
            {
                muzzleFlashLight.intensity = i;
            });
        }

        IEnumerator StopMuzzleFlashParticles()
        {
            yield return new WaitForSeconds(0.1f);
            muzzleFlashParticle?.Stop();
        }
    }
}