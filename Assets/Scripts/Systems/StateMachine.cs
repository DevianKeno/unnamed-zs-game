using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UZSG.Systems
{
    /// <summary>
    /// Base class for State Machines.
    /// </summary>
    public abstract class StateMachine<E> : MonoBehaviour where E : Enum
    {
        /// <summary>
        /// Information on State transition.
        /// </summary>
        public struct TransitionContext
        {
            public E From { get; set; }
            public E To { get; set; }
        }

        protected Dictionary<E, State<E>> _states = new();
        public Dictionary<E, State<E>> States => _states;
        public E InitialState;
        [SerializeField] protected E currentState;
        /// <summary>
        /// The current State of this StateMachine. [Read Only]
        /// </summary>
        public E CurrentState => currentState;
        [SerializeField] protected float _lockedUntil;
        public float LockedUntil => _lockedUntil;
        [SerializeField] protected bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning;
        /// <summary>
        /// Whether to allow reentry-ing the same state.
        /// If false, transitioning to the same state will do nothing, not even calling the events.
        /// It true, proceed to transition as normal even if towards the same state.
        /// </summary>
        public bool AllowReentry = false;
        public bool DebugMode = false;
        
        /// <summary>
        /// Called everytime after the State changes.
        /// Use this if you want to the events of the State Machine itself.
        /// </summary>
        public event Action<TransitionContext> OnTransition;

        public State<E> this[E state]
        {
            get
            {
                return _states[state];
            }
        }

        protected virtual void Awake()
        {
            foreach (E state in Enum.GetValues(typeof(E)))
            {
                _states[state] = new State<E>(state);
            }
            InitialState = _states.Values.First().Key;
        }

        protected virtual void Update()
        {
            // if (_currentState.EnableUpdateCall)
            // {
                _states[currentState].Update();
            // }
        }

        protected virtual void FixedUpdate()
        {
            // if (_currentState.EnableFixedUpdateCall)
            // {
                _states[currentState].FixedUpdate();
            // }
        }

        #region Public methods
        
        /// <summary>
        /// Transition to state. Pass lockForSeconds to lock this state, preventing transitions to other states for a certain amount of seconds.
        /// </summary>
        public virtual void ToState(E state, float lockForSeconds = 0f)
        {
            if (!_states.ContainsKey(state)) return;
            
            ToStateE(_states[state], lockForSeconds);
        }

        /// <summary>
        /// Lock the current state for a seconds.
        /// </summary>
        public virtual void LockForSeconds(float seconds)
        {
            _lockedUntil = Time.realtimeSinceStartup + seconds;
        }

        /// <summary>
        /// Lock the current state for a seconds.
        /// </summary>
        public virtual void Unlock()
        {
            _lockedUntil = Time.realtimeSinceStartup - 1f;
        }

        /// <summary>
        /// Get the state given the key.
        /// </summary>
        public virtual State<E> GetState(E key)
        {
            return _states[key];
        }

        /// <summary>
        /// Check if the current state is the given State.
        /// </summary>
        public virtual bool InState(E key)
        {
            return currentState.Equals(key);
        }

        #endregion

        protected virtual void ToStateE(State<E> state, float lockForSeconds)
        {
            /// Return if same state transition and does not allow re-entrying 
            if (state.Key.Equals(currentState) && !AllowReentry) return;

            if (!TrySwitchState(state, lockForSeconds))
            {
                if (DebugMode)
                {
                    Debug.Log($"Cannot switch to {state.Key} - State locked until {_lockedUntil}");
                }
            }
        }

        protected virtual bool TrySwitchState(State<E> state, float lockForSeconds = 0f)
        {
            if (Time.realtimeSinceStartup < _lockedUntil)
            {
                if (DebugMode)
                {
                    Debug.Log($"State transition to {state.Key} blocked. Locked until {_lockedUntil}");
                }
                return false;
            }

            _isTransitioning = true;
            _lockedUntil = Time.realtimeSinceStartup + lockForSeconds;

            var previousState = currentState;
            var context = new TransitionContext()
            {
                From = previousState,
                To = state.Key
            };

            _states[currentState].Exit(context);
            currentState = state.Key;
            _states[currentState].Enter(context);
            _isTransitioning = false;

            OnTransition?.Invoke(context);

            if (DebugMode) 
            {
                Debug.Log("Switched to state " + state);
            }
            
            return true;
        }
    }
}