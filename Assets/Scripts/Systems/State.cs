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
            public EState PreviousState;
            public EState NextState;
        }
        
        EState _key;
        public EState Key => _key;
        bool _isLocked;
        public bool IsLocked => _isLocked;


        #region State events

        /// <summary>
        /// Called once when entering this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnEnter;
        /// <summary>
        /// Called every game tick when in this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnTick;
        /// <summary>
        /// Called every realtime second when in this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnSecond;
        /// <summary>
        /// Called once when exiting this State.
        /// </summary>
        public event EventHandler<ChangedContext> OnExit;

        #endregion


        public State(EState key)
        {
            _key = key;
        }

        public void Lock(bool value)
        {
            _isLocked = value;
        }

        public virtual void Enter()
        {
            OnEnter?.Invoke(this, new()
            {

            });
        }

        public virtual void Tick()
        {
            OnTick?.Invoke(this, new()
            {

            });
        }

        public virtual void Second()
        {
            OnSecond?.Invoke(this, new()
            {

            });
        }

        public virtual void Exit()
        {
            OnExit?.Invoke(this, new()
            {

            });
        }
    }
}