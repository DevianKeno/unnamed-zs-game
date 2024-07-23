using System;
using UnityEngine;

namespace UZSG.Systems
{
    public struct TickInfo
    {
        /// <summary>
        /// The current tick progress index, which ranges from 0 to max TPS.
        /// </summary>
        public int Tick { get; set; }
        /// <summary>
        /// The time it took to complete this tick.
        /// </summary>
        public float DeltaTime  { get; set; }
    }

    public struct SecondInfo
    {
    }

    /// <summary>
    /// Game's internal Tick system.
    /// </summary>
    public sealed class TickSystem : MonoBehaviour
    {
        public const int MaxTPS = 10000;

        [SerializeField] int _ticksPerSecond;
        public int TPS
        {
            get { return _ticksPerSecond; }
            set
            {
                SetTPS(value);
            }
        }
        
        [SerializeField] float _secondsPerTick;
        public float SecondsPerTick => _secondsPerTick;

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
        [SerializeField] int _secondsElapsed = 0;

        float _deltaTick = 0f;
        /// <summary>
        /// The interval in seconds from the last tick to the current tick.
        /// </summary>
        public float DeltaTick => _deltaTick;

        [SerializeField] bool _isFrozen = false;
        public bool IsFrozen => _isFrozen;


        #region Events
        /// <summary>
        /// Called every game tick.
        /// </summary>
        public event Action<TickInfo> OnTick;
        /// <summary>
        /// Called every real-time second.
        /// </summary>
        public event Action<SecondInfo> OnSecond;
        
        #endregion


        void OnValidate()
        {
            _secondsPerTick = 1 / (_ticksPerSecond <= 0f ? 0.01f : _ticksPerSecond);
        }

        void Update()
        {
            if (_isFrozen) return;

            _deltaTick += Time.deltaTime;

            if (_deltaTick >= _secondsPerTick)
            {
                OnTick?.Invoke(new()
                {
                    Tick = _currentTick,
                    DeltaTime = _deltaTick,
                });

                _deltaTick -= _secondsPerTick;
                _currentTick++;
                _totalTicks++;

                if (_currentTick >= _ticksPerSecond)
                {
                    OnSecond?.Invoke(new());

                    _currentTick -= _ticksPerSecond;
                    _secondsElapsed++;
                }
            }
        }

        internal void Initialize()
        {
            SetTPS(TPS);
        }

        public void SetTPS(int value)
        {
            value = Mathf.Clamp(value, 0, MaxTPS);
            if (value == 0)
            {
                SetFrozen(true);
                return;
            }

            if (_isFrozen)
            {
                SetFrozen(false);
            }
            _ticksPerSecond = value;
            _secondsPerTick = 1f / _ticksPerSecond;
        }

        public void SetFrozen(bool value)
        {
            _isFrozen = value;

            if (value)
            {
                _deltaTick = 0f;
            }
            else
            {
                /// Reinitialize tick settings if not frozen
                _secondsPerTick = 1f / _ticksPerSecond;
            }
        }
    }
}
