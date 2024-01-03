using System;
using UnityEngine;

namespace UZSG.Systems
{
    public struct TickEventArgs
    {
        public int _tick;
        public readonly int Tick => _tick;
        public float _deltaTime;
        /// <summary>
        /// The time it took to complete this tick.
        /// </summary>
        public readonly float DeltaTime => _deltaTime;
    }

    public struct SecondEventArgs
    {
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
        [SerializeField] float _secondsPerTick;
        public float SecondsPerTick => _secondsPerTick;
        int _cachedTicksPerSecond;

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

        float _deltaTick = 0f;
        /// <summary>
        /// The interval in seconds from the last tick to the current tick.
        /// </summary>
        public float DeltaTick => _deltaTick;

        #region Events
        /// <summary>
        /// Called every game tick.
        /// </summary>
        public event EventHandler<TickEventArgs> OnTick;
        /// <summary>
        /// Called every real-time second.
        /// </summary>
        public event EventHandler<SecondEventArgs> OnSecond;
        
        #endregion

        void Update()
        {
            if (_isFrozen) return;

            _deltaTick += Time.deltaTime;

            if (_deltaTick >= _secondsPerTick)
            {
                OnTick?.Invoke(this, new TickEventArgs{
                    _tick = _currentTick,
                    _deltaTime = _deltaTick
                });

                _deltaTick -= _secondsPerTick;
                _currentTick++;
                _totalTicks++;

                if (_currentTick >= _ticksPerSecond)
                {
                    _currentTick = 0;
                    OnSecond?.Invoke(this, new SecondEventArgs());
                }
            }
        }

        internal void Initialize()
        {
            _ticksPerSecond = DefaultTPS;
            _secondsPerTick = 1f / DefaultTPS;
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
                SetFreezed(true);
                return;
            }

            if (_isFrozen) SetFreezed(false);

            if (value > MaxTPS)
            {
                _ticksPerSecond = MaxTPS;
            } else
            {
                _ticksPerSecond = value;
            }
            _secondsPerTick = 1f / _ticksPerSecond;
        }

        public void SetFreezed(bool value)
        {
            _isFrozen = value;

            if (value)
            {
                _cachedTicksPerSecond = _ticksPerSecond;
                _ticksPerSecond = 0;
                _secondsPerTick = 0f;
                _deltaTick = 0f;
            } else
            {
                _ticksPerSecond = _cachedTicksPerSecond;
                _secondsPerTick = 1f / _ticksPerSecond;
            }
        }
    }
}

