using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.Systems
{
    /// <summary>
    /// Base class for State Machines.
    /// </summary>
    public abstract class StateMachine<E> : MonoBehaviour where E : Enum
    {
        public struct StateChanged
        {
            public E From;
            public E To;
        }
        
        Dictionary<E, State<E>> _states = new();
        public Dictionary<E, State<E>> States => _states;
        public State<E> InitialState;
        [SerializeField] State<E> _currentState;
        public State<E> CurrentState => _currentState;
        [SerializeField] float _lockedUntil;
        public float LockedUntil => _lockedUntil;
        [SerializeField] bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning;
        public bool DebugMode = false;
        
        /// <summary>
        /// Calles everytime before the State changes.
        /// </summary>
        public event EventHandler<StateChanged> OnStateChanged;

        void Awake()
        {
            foreach (E state in Enum.GetValues(typeof(E)))
            {
                _states[state] = new State<E>(state);
            }
        }

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
        public virtual void ToState(State<E> state)
        {
            if (_currentState == state) return;            
            TrySwitchState(state);
        }

        public virtual void ToState(E state)
        {
            if (!_states.ContainsKey(state)) return;
            TrySwitchState(_states[state]);
        }

        /// <summary>
        /// Transition to state and prevent from transitioning to other states for a certain amount of seconds.
        /// </summary>
        public virtual void ToState(State<E> state, float lockForSeconds)
        {
            if (_currentState == state) return;
            TrySwitchState(state, lockForSeconds);
        }

        bool TrySwitchState(State<E> state, float lockForSeconds = 0f)
        {
            if (Time.time < LockedUntil) return false;
            _isTransitioning = true;
            _lockedUntil = Time.time + lockForSeconds;

            OnStateChanged?.Invoke(this, new()
            {
                From = _currentState.Key,
                To = state.Key
            });
            _currentState.Exit();
            _currentState = state;
            _currentState.Enter();
            _isTransitioning = false;
            if (DebugMode) Debug.Log("Switched to state " + state);
            return true;
        }
    }
}