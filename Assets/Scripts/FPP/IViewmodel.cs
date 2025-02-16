using UnityEngine.AddressableAssets;

using UZSG.Data;

namespace UZSG.FPP
{
    /// <summary>
    /// Represent items that are visible on first-person perspective.
    /// </summary>
    public interface IViewmodelItem
    {
        public bool HasViewmodel { get; }
        public AssetReference ViewmodelAsset { get; }
        public ViewmodelSettings ViewmodelSettings { get; }
        public EquipmentAnimationData AnimationsData { get; }
    }
}