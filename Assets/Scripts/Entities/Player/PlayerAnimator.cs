using System;
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

        [SerializeField] float _targetX = 0f;
        [SerializeField] float _targetY = 0f;

        [SerializeField] protected Animator animator;
        public Animator Animator => animator;
        [SerializeField] protected Animator animatorFPP;
        public Animator AnimatorFPP => animatorFPP;
        
        void OnMoveStateChanged(object sender, StateMachine<MoveStates>.StateChangedContext e)
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

        string GetAnimationName(Enum e)
        {
            return e.ToString().ToLower();
        }
    }
}