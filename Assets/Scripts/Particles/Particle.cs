using System;

using UnityEngine;

namespace UZSG.Systems
{
    public class Particle : MonoBehaviour
    {
        [SerializeField] protected new ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem => particleSystem;
        [SerializeField] protected ParticleSystemRenderer particleSystemRenderer;
        public ParticleSystemRenderer ParticleSystemRenderer => particleSystemRenderer;
    }
}
