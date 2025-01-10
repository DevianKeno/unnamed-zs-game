using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Tile Data", menuName = "UZSG/Objects/Tile Data")]
    public class TileData : ObjectData
    {
        [Header("Tile Data")]
        public bool IsLockedToGrid;
        public bool IsAnimated;

    }
}