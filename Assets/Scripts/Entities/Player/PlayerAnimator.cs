using System;
using System.Collections;
using UnityEngine;
using UZSG.Players;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// <summary>
    /// Player animator functionalities.
    /// </summary>
    public partial class Player : Entity
    {
        [Header("Animator")]
        public float Damping = 1f;
        public float CrossfadeTransitionDuration = 0.5f;

        bool _isPlayingAnimation = false;
        [SerializeField] float _targetX = 0f;
        [SerializeField] float _targetY = 0f;

        [SerializeField] protected Animator animator;
        public Animator Animator => animator;
        [SerializeField] protected Animator animatorFPP;
        public Animator AnimatorFPP => animatorFPP;
        
        void InitializeAnimator()
        {
            MoveStateMachine[MoveStates.Crouch].OnTransition += OnCrouchState;
        }

        void OnCrouchState(StateMachine<MoveStates>.TransitionContext e)
        {
            if (e.From == MoveStates.Crouch)
            {
                print("exited crouch");
                animator.CrossFade($"crouch_to_stand", CrossfadeTransitionDuration);
            }
            else if (e.To == MoveStates.Crouch)
            {
                print("entered crouch");
                animator.CrossFade($"stand_to_crouch", CrossfadeTransitionDuration);
            }
        }

        void OnMoveStateChanged(StateMachine<MoveStates>.TransitionContext e)
        {
            var animId = GetAnimationName(e.To);
            animator.CrossFade($"{animId}", CrossfadeTransitionDuration);
            animatorFPP.CrossFade($"{animId}", CrossfadeTransitionDuration);
        }

        void Update()
        {
            float x = Controls.FrameInput.Move.x;
            float y = Controls.FrameInput.Move.y;
            _targetX = Mathf.Lerp(Animator.GetFloat("x"), x, Damping * Time.deltaTime);
            _targetY = Mathf.Lerp(Animator.GetFloat("y"), y, Damping * Time.deltaTime);

            animator.SetFloat("x", _targetX);
            animator.SetFloat("y", _targetY);

            animatorFPP.SetFloat("x", _targetX);
            animatorFPP.SetFloat("y", _targetY);
        }
        
        IEnumerator FinishAnimation(float durationSeconds)
        {
            if (_isPlayingAnimation) yield return null;
            _isPlayingAnimation = true;

            yield return new WaitForSeconds(durationSeconds);
            _isPlayingAnimation = false;
            yield return null;
        }

        string GetAnimationName(Enum e)
        {
            return e.ToString().ToLower();
        }
        
        float GetAnimationClipLength(Animator animator, string name)
        {
            #region TODO:change this to a flag
            #endregion
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == name) return clip.length;
            }
            return 0f;
        }
    }
}