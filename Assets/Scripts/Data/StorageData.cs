using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Storage Data", menuName = "UZSG/Objects/Storage Data")]
    public class StorageData : ObjectData
    {
        [Header("Storage Data")]
        public string StorageName;
        public AssetReference GUI;
        public int Size;
    }
}