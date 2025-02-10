using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UZSG.Data
{
    /// <summary>
    /// Data for storage container objects.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Storage Data", menuName = "UZSG/Objects/Storage Data")]
    public class StorageData : ObjectData
    {
        [Header("Storage Data")]
        public string StorageName;
        public string StorageNameTranslatable => Game.Locale.Translatable($"object.{Id}.name");
        public int Size;
        public AssetReference GUIAsset;
    }
}