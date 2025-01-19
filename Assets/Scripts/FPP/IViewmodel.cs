using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Data;

namespace UZSG.FPP
{
    /// <summary>
    /// For objects that is visible on FPP.
    /// </summary>
    public interface IViewmodel
    {
        public RuntimeAnimatorController ArmsAnimations { get; }
        public AssetReference Viewmodel { get; }
        public ViewmodelSettings Settings { get; }
        public EquipmentAnimationData Animations { get; }
        public bool HasViewmodel { get; }
    }
}