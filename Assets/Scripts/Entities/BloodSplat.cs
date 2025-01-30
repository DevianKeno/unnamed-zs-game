using UnityEngine;

namespace UZSG.Particles
{
    public class BloodSplat : MonoBehaviour
    {
        [SerializeField] ParticleSystem bloodSplat;
        [SerializeField] ParticleSystem bloodCloud;

        void Start()
        {
            bloodSplat.Play();
            bloodCloud.Play();
            Destroy(gameObject, 3f);
        }
    }
}