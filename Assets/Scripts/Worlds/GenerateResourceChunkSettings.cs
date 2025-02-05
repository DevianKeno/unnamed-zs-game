using System;

namespace UZSG.Worlds
{
    [Serializable]
    public struct GenerateResourceChunkSettings
    {
        public bool PlaceTrees { get; set; }
        public float TreeDensity { get; set; }
        public bool PlacePickups { get; set; }
        public float PickupsDensity { get; set; }
    }
}
