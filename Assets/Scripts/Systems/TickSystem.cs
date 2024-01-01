using System;
using UnityEngine;

namespace UZSG.Systems
{
    public struct TickEventArgs
    {
        public int Tick;
    }
    /// <summary>
    /// Game's internal Tick system.
    /// </summary>
    public sealed class TickSystem : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] int DefaultTPS = 60;
        [SerializeField] int MaxTPS = 10000;
        
        [Header("Current")]
        [SerializeField] bool _isFrozen = false;
        public bool IsFrozen => _isFrozen;
        [SerializeField] int _ticksPerSecond;
        public int TicksPerSecond => _ticksPerSecond;
        public int TPS => _ticksPerSecond;
        [SerializeField] float _secondPerTick;
        public float SecondPerTick => _secondPerTick;
        float _tickTimer;

        [Header("Lifetime")]
        [SerializeField] int _currentTick;
        /// <summary>
        /// The tick in the current second, which is a number between 0 and TicksPerSecond.
        /// </summary>
        public int CurrentTick => _currentTick;
        [SerializeField] int _totalTicks;
        /// <summary>
        /// Total number of ticks.
        /// </summary>
        public int TotalTicks => _totalTicks;

        #region Events
        /// <summary>
        /// Called every game tick.
        /// </summary>
        public event EventHandler<TickEventArgs> OnTick;
        /// <summary>
        /// Called every real-time second.
        /// </summary>
        public event EventHandler<TickEventArgs> OnSecond;
        
        #endregion

        void Update()
        {
            if (_isFrozen) return;

            _tickTimer += Time.deltaTime;

            if (_tickTimer >= _secondPerTick)
            {
                _tickTimer -= _secondPerTick;
                _currentTick++;
                _totalTicks++;

                OnTick?.Invoke(this, new TickEventArgs{ Tick = _currentTick });

                if (_currentTick >= _ticksPerSecond)
                {
                    _currentTick = 0;
                    OnSecond?.Invoke(this, new TickEventArgs{ Tick = _currentTick });
                }
            }
        }

        internal void Initialize()
        {
            _ticksPerSecond = DefaultTPS;
            _secondPerTick = 1f / DefaultTPS;
        }

        public void SetTPS(int value)
        {
            if (value < 0)
            {
                Debug.LogWarning("TPS cannot be a negative value.");
                return;
            }

            if (value == 0)
            {
                ToggleFreeze(true);
                return;
            }

            if (_isFrozen) ToggleFreeze(false);

            if (value > MaxTPS)
            {
                _ticksPerSecond = MaxTPS;
            } else
            {
                _ticksPerSecond = value;
            }
            _secondPerTick = 1f / _ticksPerSecond;
        }

        public void ToggleFreeze(bool value)
        {
            _isFrozen = value;
        }
    }
}

