using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Level Data", menuName = "UZSG/Level Data")]
    public class LevelData : BaseData
    {
        public string Name;
        [TextArea] public string Description;
        public AssetReference Asset;
    }
}