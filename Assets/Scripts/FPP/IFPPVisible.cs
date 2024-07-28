using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public AnimatorController ArmsAnimations { get; }
        public AssetReference Viewmodel { get; }
        public EquipmentAnimationData Anims { get; }
        public bool HasViewmodel { get; }
        public Vector3 PositionOffset { get; }
        public Vector3 RotationOffset { get; }
    }

}
