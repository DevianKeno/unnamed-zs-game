using System;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace UZSG.Data
{
    /// <summary>
    /// Level data.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Level Data", menuName = "UZSG/Level Data")]
    public class LevelData : BaseData
    {
        [FormerlySerializedAs("Name")] public string DisplayName;
        [TextArea] public string Description;
        public Vector2 DimensionsKilometers;
        public bool Enable = true;
        public Texture Image;
        public string SceneName; /// Do not leave blank
        public AssetReference Scene;
    }
}