using UnityEngine;

namespace UZSG.FPP
{
    public class FPPArmsController : MonoBehaviour
    {
        /// <summary>
        /// Static AnimatorController field for Viewmodel with no arms animations :)
        /// </summary>
        [SerializeField] RuntimeAnimatorController noAnimations;
        [SerializeField] Animator animator;
        [SerializeField] Transform armsHolder;

        public void SetAnimatorController(RuntimeAnimatorController controller)
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
        public void SetViewmodelSettings(ViewmodelSettings settings)
        {
            if (settings.UseOffsets)
            {
                armsHolder.SetLocalPositionAndRotation(settings.PositionOffset, Quaternion.Euler(settings.RotationOffset));
            }
        }
    }
}