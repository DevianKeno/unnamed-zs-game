using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Object Data", menuName = "UZSG/Object Data")]
    public class ObjectData : BaseData
    {
        public string Name;
        public AssetReference Model;
    }
}