using System;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    public struct AudioAssetId
    {
        public string Id;
        public AssetReference AudioAsset;
    }
}