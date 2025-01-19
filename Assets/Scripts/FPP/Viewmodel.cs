using System;

using UnityEngine;

using UZSG.Data;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    [Serializable]
    public class Viewmodel
    {
        [field: SerializeField] public ItemData ItemData { get; set; }
        [field: SerializeField] public GameObject Model { get; set; }
        [field: SerializeField] public ViewmodelSettings Settings { get; set; }
        [field: SerializeField] public RuntimeAnimatorController ArmsAnimations { get; set; }
        [field: SerializeField] public Animator ModelAnimator { get; set; }
        [field: SerializeField] public Animator CameraAnimator { get; set; }
        [field: SerializeField] public FPPCameraAnimationSource CameraAnimationSource { get; set; }
    }
}