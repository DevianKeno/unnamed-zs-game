using UnityEditor.Animations;
using UnityEngine.AddressableAssets;

using UZSG.Data;

namespace UZSG.FPP
{
    public interface IViewmodel
    {
        public AnimatorController ArmsAnimations { get; }
        public AssetReference Viewmodel { get; }
        public ViewmodelOffsets Offsets { get; }
        public EquipmentAnimationData Animations { get; }
        public bool HasViewmodel { get; }
    }
}