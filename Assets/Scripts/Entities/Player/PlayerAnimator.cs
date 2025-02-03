using System;
using System.Collections;
using UnityEngine;
using UZSG.Players;
using UZSG.Systems;

namespace UZSG.Entities
{
    /// Player animator functionalities.
    public partial class Player : Entity
    {
        [Header("Animator")]
        public float Damping = 1f;
        public float CrossfadeTransitionDuration = 0.5f;

        bool _isPlayingAnimation = false;
        bool _enableModelAnimations = true;
        bool _enableClientAnimations = false;
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

        void InitializeAnimatorAsClient()
        {
            animatorFPP.gameObject.SetActive(true);
            _enableClientAnimations = true;

            animator.gameObject.SetActive(false);
            _enableModelAnimations = false;
        }


        #region Event callbacks (all over)

        void OnCrouch(bool crouched)
        {
            if (crouched)
            {
                // print("entered crouch");
                AnimateTogether($"stand_to_crouch", 0f); /// no transition fade
            }
            else
            {
                // print("exited crouch");
                AnimateTogether($"crouch_to_stand", 0f); /// no transition fade
            }
        }

        void OnTurn(float direction)
        {
            AnimateTogether($"turn", 0f);

            if (_enableModelAnimations)
            {
                animator.SetFloat("turn", direction);
            }
            if (_enableClientAnimations)
            {
                animatorFPP.SetFloat("turn", direction);
            }
        }

        void OnInteractVehicle(VehicleInteractContext context)
        {
            /// this can handle entering/exiting vehicle animations but good luck with that lmao
            if (context.Entered)
            {
                // print("Entered vehicle");
                AnimateTogether($"car_idle", 0f); /// no transition fade
            }
            else if (context.Exited)
            {
                // print("Exited vehicle");
                AnimateTogether($"idle", 0f); /// no transition fade
            }
        }

        #endregion

        
        /// <summary>
        /// Animates both the TPP and FPP player models.
        /// </summary>
        void AnimateTogether(string anim, float crossfadeTransitionDuration)
        {
            if (_enableModelAnimations)
            {
                animator.CrossFade(anim, crossfadeTransitionDuration);
            }
            if (_enableClientAnimations)
            {
                animatorFPP.CrossFade(anim, crossfadeTransitionDuration);
            }
        }

        void TransitionAnimator(StateMachine<MoveStates>.TransitionContext t)
        {
            var animId = GetAnimationName(t.To);

            AnimateTogether($"{animId}", CrossfadeTransitionDuration);
        }

        void UpdateAnimator()
        {
            float x = Controls.FrameInput.Move.x;
            float y = Controls.FrameInput.Move.y;

            if (_enableModelAnimations)
            {
                animator.SetFloat("x", x);
                animator.SetFloat("y", y);
            }
            if (_enableClientAnimations)
            {
                animatorFPP.SetFloat("x", x);
                animatorFPP.SetFloat("y", y);
            }
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