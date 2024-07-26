using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace UZSG.FPP
{
    public class FPPArmsController : MonoBehaviour
    {
        [SerializeField] AnimatorController noAnimations;
        [SerializeField] Animator animator;

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
    }
}