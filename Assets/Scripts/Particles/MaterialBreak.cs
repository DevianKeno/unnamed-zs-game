using UnityEngine;

using UZSG.Particles;

namespace UZSG
{
    public class MaterialBreak : Particle
    {
        public void SetMaterial(Material material)
        {
            this.particleSystemRenderer.material = material;
        }
    }
}
