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
        public Vector2 DimensionsKilometers;
        public bool Enable = true;
        public Texture Image;
        public AssetReference Scene;
    }
}