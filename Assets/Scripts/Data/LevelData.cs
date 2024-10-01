using System;
using UnityEditor;
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
        public string Dimensions;
        public Texture Image;
        public AssetReference Scene;
    }
}