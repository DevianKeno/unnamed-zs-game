using System;

namespace UZSG.Systems
{
    /// <summary>
    /// Base state representation.
    /// </summary>
    [Serializable]
    public class State<EState> where EState : Enum
    {
        /// <summary>
        /// The enum value of this State.
        /// </summary>
        public EState Key { get; private set; }
        /// <summary>
        /// Enable if you want this State to call the OnUpdate event every Unity Update.
        /// Disabled by default.
        /// </summary>
        public bool EnableUpdateCall { get; set; } = false;
        /// <summary>
        /// Enable if you want this State to call the OnFixedUpdate event every Unity FixedUpdate.
        /// Disabled by default.
        /// </summary>
        public bool EnableFixedUpdateCall { get; set; } = false;


        #region State events

        /// <summary>
        /// Called once when transitioning to this State.
        /// Use this if you want to use individual State Events.
        /// </summary>
        public event Action<StateMachine<EState>.TransitionContext> OnTransition;
        /// <summary>
        /// Called every Unity's Update cycle.
        /// Use this if you want to use individual State Events.
        /// Don't forget to set EnableUpdateCall to <c>true</c>.
        /// </summary>
        public event Action OnUpdate;
        /// <summary>
        /// Called every Unity's FixedUpdate cycle.
        /// Use this if you want to use individual State Events.
        /// Don't forget to set EnableFixedUpdateCall to <c>true</c>.
        /// </summary>
        public event Action OnFixedUpdate;

        #endregion


        public State(EState key)
        {
            Key = key;
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