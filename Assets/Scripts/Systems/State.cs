using System;
using UnityEngine;
using UnityEngine.Events;

namespace UZSG.Systems
{
    /// <summary>
    /// Base state representation.
    /// </summary>
    [Serializable]
    public class State<EState> where EState : Enum
    {
        public struct ChangedContext
        {
            public EState From { get; set; }
            public EState To { get; set; }
        }
        
        EState _key;
        /// <summary>
        /// The enum value of this State.
        /// </summary>
        public EState Key => _key;


        #region State events

        /// <summary>
        /// Called once when transitioning to this State.
        /// </summary>
        public event Action<StateMachine<EState>.TransitionContext> OnTransition;
        /// <summary>
        /// Called every Unity's Update cycle.
        /// </summary>
        public event Action OnUpdate;
        /// <summary>
        /// Called every Unity's FixedUpdate cycle.
        /// </summary>
        public event Action OnFixedUpdate;

        #endregion


        public State(EState key)
        {
            _key = key;
        }

        public virtual void Enter(StateMachine<EState>.TransitionContext context)
        {
            OnTransition?.Invoke(context);
        }

        public virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        public virtual void FixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        public virtual void Second()
        {
            OnUpdate?.Invoke();
        }

        public virtual void Exit(StateMachine<EState>.TransitionContext context)
        {
            OnTransition?.Invoke(context);
        }
    }
}