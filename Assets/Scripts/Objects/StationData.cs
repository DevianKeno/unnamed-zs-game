using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Data;

namespace UZSG.Objects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Station Data", menuName = "UZSG/Station Data")]
    public class StationData : ObjectData
    {
        public AssetReference GUI;
    }
}