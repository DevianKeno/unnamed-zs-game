using System;

using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UZSG.Items;
using UZSG.Items.Weapons;

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

    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    [Serializable]
    public class Viewmodel
    {
        [field: SerializeField] public ItemData ItemData { get; set; }
        [field: SerializeField] public GameObject Model { get; set; }
        [field: SerializeField] public AnimatorController ArmsAnimations { get; set; }
        [field: SerializeField] public Animator ModelAnimator { get; set; }
        [field: SerializeField] public Animator CameraAnimator { get; set; }
        [field: SerializeField] public FPPCameraAnimationSource CameraAnimationSource { get; set; }
    }
}