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
        public GameObject ArmsModel { get; }
        public GameObject Model { get; }
        public AnimatorController ArmsController { get; }
        public AnimatorController ModelController { get; }
        public FPPAnimations Anims { get; }
    }

}
