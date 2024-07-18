using UnityEditor.Animations;
using UnityEngine;
using UZSG.Items;

namespace UZSG.FPP
{
    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public GameObject Model { get; }
        public AnimatorController ArmsAnimController { get; }
        // public AnimatorController ArmsController { get; }
        public AnimatorController ModelAnimController { get; }
        public FPPAnimations Anims { get; }
    }

}
