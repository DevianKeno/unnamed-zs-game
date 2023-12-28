using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.World
{
    public struct Voxel
    {
        public int Id;

        public static Voxel Empty
        {
            get => new() { Id = 0 };
        }

        public bool IsSolid
        {
            get => Id != 0;
        }
    }
}
