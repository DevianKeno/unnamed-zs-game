using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UZSG.Data;
using UZSG.Items;

namespace UZSG.Systems
{
    public class MaterialBreak: Particle
    {
        public void SetMaterial(Material material)
        {
            this.particleSystemRenderer.material = material;
        }
    }
}
