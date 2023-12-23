using System;
using System.Collections.Generic;
using UnityEngine;
using URMG.Systems;
using URMG.States;

namespace URMG
{
    public struct StateChangedArgs
    {
        public State Current;
        public State Next;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class StateMachine : MonoBehaviour
    {
        public State InitialState;
        [SerializeField] State _currentState;
        public State CurrentState { get => _currentState; }
        public float LockedUntil;
        public bool IsTransitioning = false;
        public List<State> States = new();

        /// <summary>
        /// Calles everytime before the state changes.
        /// </summary>
        public event EventHandler<StateChangedArgs> OnStateChanged;

        void Start()
        {
            if (InitialState != null) _currentState = InitialState;

            Game.Tick.OnTick += Tick;
        }

        void Tick(object sender, TickEventArgs e)
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
            OnStateChanged?.Invoke(this, new()
            {
                Current = _currentState,
                Next = state
            });
            _currentState.Exit();
            _currentState = state;
            _currentState.Enter();

            LockedUntil = Time.time + lockForSeconds;
            IsTransitioning = false;

            return true;
        }
    }
}