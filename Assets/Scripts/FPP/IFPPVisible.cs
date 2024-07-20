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
        public AssetReference WeaponViewmodel { get; }
        public EquipmentAnimationData Anims { get; }
    }

}
