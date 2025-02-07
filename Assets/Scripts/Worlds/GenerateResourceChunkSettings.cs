using System;

namespace UZSG.Worlds
{
    [Serializable]
    public struct GenerateResourceChunkSettings
    {
        public bool PlaceTrees { get; set; }
        public bool PlacePickups { get; set; }
    }
}
