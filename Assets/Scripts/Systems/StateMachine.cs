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
        /// <summary>
        /// Information on State transition.
        /// </summary>
        public struct TransitionContext
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
        /// <summary>
        /// The current State of this StateMachine. [Read Only]
        /// </summary>
        public E InState;
        public bool DebugMode = false;
        
        /// <summary>
        /// Called everytime before the State changes.
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

        void Awake()
        {
            foreach (E state in Enum.GetValues(typeof(E)))
            {
                _states[state] = new State<E>(state);
            }

            /// Set InitialState to the first enum value if it's not manually set
            InitialState ??= _states[(E)Enum.GetValues(typeof(E)).GetValue(0)];
        }

        void Start()
        {
            /// If InitialState is not null, set _currentState to it
            if (InitialState != null)
            {
                _currentState = InitialState;
            }
            else
            {
                Game.Console.LogAndUnityLog("InitialState is not set, StateMachine will not start with a valid state.");
            }
        }

        void Update()
        {
            if (_currentState.EnableUpdateCall)
            {
                _currentState.Update();
            }
        }

        void FixedUpdate()
        {
            if (_currentState.EnableFixedUpdateCall)
            {
                _currentState.FixedUpdate();
            }
        }

        #region Public methods
        
        /// <summary>
        /// Transition to state. Pass lockForSeconds to lock this state, preventing transitions to other states for a certain amount of seconds.
        /// </summary>
        public void ToState(E state, float lockForSeconds = 0f)
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

        #endregion

        void ToStateE(State<E> state, float lockForSeconds)
        {
            /// Return if same state transition and does not allow re-entrying 
            if (state.Key.Equals(_currentState.Key) && !AllowReentry) return;

            if (!TrySwitchState(state, lockForSeconds))
            {
                if (DebugMode)
                {
                    Debug.Log($"Cannot switch to {state.Key} - State locked until {_lockedUntil}");
                }
            }
        }

        bool TrySwitchState(State<E> state, float lockForSeconds = 0f)
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

            var context = new TransitionContext()
            {
                From = _currentState.Key,
                To = state.Key
            };
            OnTransition?.Invoke(context);
            
            _currentState.Exit(context);
            _currentState = state;
            InState = state.Key;
            _currentState.Enter(context);
            _isTransitioning = false;

            if (DebugMode) 
            {
                Debug.Log("Switched to state " + state);
            }
            
            return true;
        }
    }
}