using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UZSG.Systems
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class StateMachine<EState> : MonoBehaviour where EState : Enum
    {
        public struct StateChangedArgs
        {
            public State<EState> Current;
            public State<EState> Next;
        }
        public Dictionary<EState, State<EState>> States = new();
        public State<EState> InitialState;
        [SerializeField] State<EState> _currentState;
        public State<EState> CurrentState => _currentState;
        [SerializeField] float _lockedUntil;
        public float LockedUntil => _lockedUntil;
        [SerializeField] bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning;
        
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
            if (InitialState != null) _currentState.Tick();
        }

        /// <summary>
        /// Transition to state.
        /// </summary>
        public virtual void ToState(State<EState> state)
        {
            if (_currentState == state) return;
            
            TrySwitchState(state);
        }

        public virtual void ToState(State<EState> state, float lockForSeconds)
        {
            if (_currentState == state) return;

            TrySwitchState(state, lockForSeconds);
        }

        bool TrySwitchState(State<EState> state, float lockForSeconds = 0f)
        {
            if (Time.time < LockedUntil) return false;
            _isTransitioning = true;
            _lockedUntil = Time.time + lockForSeconds;

            OnStateChanged?.Invoke(this, new()
            {
                Current = _currentState,
                Next = state
            });
            _currentState.Exit();
            _currentState = state;
            _currentState.Enter();

            _isTransitioning = false;

            return true;
        }
    }
}