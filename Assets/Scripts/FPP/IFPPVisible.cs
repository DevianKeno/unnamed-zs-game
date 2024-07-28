using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    [Serializable]
    public struct ViewmodelOffsets
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }

    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public AnimatorController ArmsAnimations { get; }
        public AssetReference Viewmodel { get; }
        public ViewmodelOffsets ViewmodelOffsets { get; }
        public EquipmentAnimationData Anims { get; }
        public bool HasViewmodel { get; }
    }

}
