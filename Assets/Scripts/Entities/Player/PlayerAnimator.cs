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
        /// <summary>
        /// Animator for the complete player model.
        /// </summary>
        public Animator Animator => animator;
        /// <summary>
        /// Animator for the culled (without upper body) player model.
        /// </summary>
        [SerializeField] protected Animator animatorFPP;
        public Animator AnimatorFPP => animatorFPP;
        
        void InitializeAnimator()
        {
            Controls.OnCrouch += OnCrouch;
            Controls.OnTurn += OnTurn;
            Actions.OnInteractVehicle += OnInteractVehicle;
        }


        #region Event callbacks (all over)

        void OnCrouch(bool crouched)
        {
            if (crouched)
            {
                // print("entered crouch");
                animator.CrossFade($"stand_to_crouch", 0f); /// no transition fade
            }
            else
            {
                // print("exited crouch");
                animator.CrossFade($"crouch_to_stand", 0f); /// no transition fade
            }
        }

        void OnTurn(float direction)
        {
            animator.CrossFade($"turn", 0f);
            animator.SetFloat("turn", direction);
        }

        void OnInteractVehicle(VehicleInteractContext context)
        {
            /// this can handle entering/exiting vehicle animations but good luck with that lmao
            if (context.Entered)
            {
                animator.CrossFade($"invehicle", 0f); /// no transition fade
            }
            else if (context.Exited)
            {
                animator.CrossFade($"idle", 0f); /// no transition fade
            }
        }

        #endregion


        void TransitionAnimator(StateMachine<MoveStates>.TransitionContext t)
        {
            var animId = GetAnimationName(t.To);

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