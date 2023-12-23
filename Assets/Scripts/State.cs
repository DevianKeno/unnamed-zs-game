using System;
using UnityEngine;
using UnityEngine.Events;

namespace UZSG.States
{
    [Serializable] public class EnterStateEvent : UnityEvent {}
    [Serializable] public class TickStateEvent : UnityEvent {}
    [Serializable] public class ExitStateEvent : UnityEvent {}

    [Serializable]
    public abstract class State : MonoBehaviour
    {
        public abstract string Name { get; }
        public int AnimId;
        public bool IsLocked;
        [SerializeField] EnterStateEvent _OnEnterState = new();
        [SerializeField] TickStateEvent _OnTickState = new();
        [SerializeField] ExitStateEvent _OnExitState = new();

        public EnterStateEvent OnEnterState
        {
            get { return _OnEnterState; }
            set { _OnEnterState = value; }
        }
        
        public TickStateEvent OnTickState
        {
            get { return _OnTickState; }
            set { _OnTickState = value; }
        }

        public ExitStateEvent OnExitState
        {
            get { return _OnExitState; }
            set { _OnExitState = value; }
        }

        public void Lock(bool value)
        {
            IsLocked = value;
        }

        /// <summary>
        /// Called once upon entering this state.
        /// </summary>
        public virtual void Enter() {}

        /// <summary>
        /// Called once every game tick while in this state.
        /// </summary>
        public virtual void Update() {}

        /// <summary>
        /// Called once upon exiting this state.
        /// </summary>
        public virtual void Exit() {}
    }
}