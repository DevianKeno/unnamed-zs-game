using UnityEngine.AddressableAssets;

using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public AssetReference ArmsViewmodel { get; }
        public AssetReference ModelViewmodel { get; }
        public EquipmentAnimationData Anims { get; }
        public bool HasViewmodel { get; }
    }

}
