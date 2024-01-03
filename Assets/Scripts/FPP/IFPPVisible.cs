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
        public GameObject FPPModel { get; }
        public AnimatorController Controller { get; }
        public FPPAnimations Anims { get; }
    }

}
