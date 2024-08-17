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
        public struct StateChangedContext
        {
            public E From { get; set; }
            public E To { get; set; }
        }
        
        Dictionary<E, State<E>> _states = new();
        public Dictionary<E, State<E>> States => _states;
        public State<E> InitialState;
        [SerializeField] State<E> _currentState;
        public State<E> CurrentState => _currentState;
        [SerializeField] protected float _lockedUntil;
        public float LockedUntil => _lockedUntil;
        [SerializeField] protected bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning;
        /// <summary>
        /// Whether to allow reentry-ing the state.
        /// </summary>
        public bool AllowReentry = false;

        public E InState;
        public bool DebugMode = false;
        
        /// <summary>
        /// Calles everytime before the State changes.
        /// </summary>
        public event EventHandler<StateChangedContext> OnStateChanged;

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

        void Tick(TickInfo e)
        {
            if (InitialState != null) _currentState.Tick();
        }

        public virtual void ToState(E state)
        {
            if (!_states.ContainsKey(state)) return;

            ToState(_states[state]);
        }

        /// <summary>
        /// Transition to state.
        /// </summary>
        public virtual void ToState(State<E> state)
        {
            if (state.Key.Equals(_currentState.Key) && !AllowReentry) return;

            TrySwitchState(state);
        }

        /// <summary>
        /// Transition to state and prevent from transitioning to other states for a certain amount of seconds.
        /// </summary>
        public virtual void ToState(State<E> state, float lockForSeconds)
        {
            TrySwitchState(state, lockForSeconds);
        }
        
        public virtual void ToState(E state, float lockForSeconds)
        {
            if (!_states.ContainsKey(state)) return;
            ToState(_states[state], lockForSeconds);
        }

        public virtual void LockForSeconds(float seconds)
        {
            _lockedUntil = Time.time + seconds;
        }

        bool TrySwitchState(State<E> state, float lockForSeconds = 0f)
        {
            if (Time.time < _lockedUntil)
            {
                return false;
            }
            _isTransitioning = true;
            _lockedUntil = Time.time + lockForSeconds;

            OnStateChanged?.Invoke(this, new()
            {
                From = _currentState.Key,
                To = state.Key
            });
            _currentState.Exit();
            _currentState = state;
            InState = state.Key;
            _currentState.Enter();
            _isTransitioning = false;
            if (DebugMode) 
            {
                Debug.Log("Switched to state " + state);
            }
            return true;
        }
    }
}