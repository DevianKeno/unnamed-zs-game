using System;
using UnityEngine;

namespace URMG
{
    public struct StateChangedArgs
    {
        public State Prev;
        public State Next;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class StateMachine : MonoBehaviour
    {
        public State InitialState;
        State _currentState;
        public State CurrentState { get => _currentState; }
        public float LockedUntil;
        public bool IsTransitioning = false;

        /// <summary>
        /// Fired everytime the state is changed.
        /// </summary>
        public event EventHandler<StateChangedArgs> OnStateChanged;

        void Start()
        {
            if (InitialState != null) _currentState = InitialState;
        }

        void Update()
        {
            _currentState.Update();
        }

        public virtual void SetState(State state)
        {
            if (_currentState == state) return;
            
            TrySwitchState(state);
        }

        public virtual void SetState(State state, float lockForSeconds)
        {
            if (_currentState == state) return;

            TrySwitchState(state, lockForSeconds);
        }

        bool TrySwitchState(State state, float lockForSeconds = 0f)
        {
            if (Time.time < LockedUntil) return false;

            IsTransitioning = true;
            _currentState.Exit();
            _currentState = state;
            _currentState.Enter();

            LockedUntil = Time.time + lockForSeconds;
            IsTransitioning = false;

            OnStateChanged?.Invoke(this, new()
            {
                Next = state
            });

            return true;
        }
    }
}