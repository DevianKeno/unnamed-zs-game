using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace UZSG.FPP
{
    public class FPPArmsController : MonoBehaviour
    {
        /// <summary>
        /// Static AnimatorController field for Viewmodel with no arms animations :)
        /// </summary>
        [SerializeField] AnimatorController noAnimations;
        [SerializeField] Animator animator;
        [SerializeField] Transform armsHolder;

        public void SetAnimatorController(AnimatorController controller)
        {
            if (controller == null)
            {
                animator.runtimeAnimatorController = noAnimations;
            }
            else
            {
                animator.runtimeAnimatorController = controller;
            }
        }

        public void PlayAnimation(string animId)
        {
            if (string.IsNullOrEmpty(animId)) return;

            animator.CrossFade(animId, 0.1f, 0, 0f);
        }

        /// <summary>
        /// Realigns the viewmodel arms given its offset values.
        /// </summary>
        public void SetTransformOffset(ViewmodelOffsets offsets)
        {
            armsHolder.SetLocalPositionAndRotation(offsets.Position, Quaternion.Euler(offsets.Rotation));
        }
    }
}