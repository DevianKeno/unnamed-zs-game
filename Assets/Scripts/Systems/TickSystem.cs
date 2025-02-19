using System;
using UnityEngine;

namespace UZSG
{
    public struct TickInfo
    {
        /// <summary>
        /// The tick's 'index', if you need it. Number between 0 and TPS.
        /// </summary>
        public int Tick { get; set; }
        /// <summary>
        /// The interval in seconds from the last tick to the current tick :)
        /// </summary>
        public float DeltaTime { get; set; }
    }

    public struct SecondInfo
    {
    }

    public sealed class TickSystem : MonoBehaviour
    {
        public const int MaxTPS = 10000;
        public const int NormalTPS = 64;

        [SerializeField] int _ticksPerSecond;
        public int TPS
        {
            get { return _ticksPerSecond; }
            set { SetTPS(value); }
        }

        [SerializeField] float _secondsPerTick;
        public float SecondsPerTick => _secondsPerTick;

        [Header("Lifetime")]
        [SerializeField] int _currentTick;
        public int CurrentTick => _currentTick;
        [SerializeField] int _totalTicks;
        public int TotalTicks => _totalTicks;
        [SerializeField] int _secondsElapsed = 0;

        float _deltaTick = 0f;
        public float DeltaTick => _deltaTick;
        [SerializeField] float _deltaSPT;
        public float DeltaSPT => Mathf.Lerp(0, _secondsPerTick, (float)_currentTick / _ticksPerSecond);

        [SerializeField] bool _isFrozen = false;
        public bool IsFrozen => _isFrozen;

        private float _lastRealTime;

        #region Events
        /// <summary>
        /// Raised every tick, (TPS) times every second.
        /// </summary>
        public event Action<TickInfo> OnTick;
        /// <summary>
        /// Raised every in game second relative to the game's tick.
        /// </summary>
        public event Action<SecondInfo> OnSecond;
        #endregion

        void OnValidate()
        {
            _secondsPerTick = 1f / (_ticksPerSecond <= 0 ? 0.01f : _ticksPerSecond);
        }

        void Start()
        {
            _lastRealTime = Time.realtimeSinceStartup;
        }

        void Update()
        {
            if (_isFrozen) return;

            float currentTime = Time.realtimeSinceStartup;
            float elapsedTime = currentTime - _lastRealTime;
            _lastRealTime = currentTime;

            _deltaTick += elapsedTime;

            int ticksToProcess = Mathf.FloorToInt(_deltaTick / _secondsPerTick);
            if (ticksToProcess > 0)
            {
                _deltaTick -= ticksToProcess * _secondsPerTick;
                for (int i = 0; i < ticksToProcess; i++)
                {
                    OnTick?.Invoke(new TickInfo
                    {
                        Tick = _currentTick,
                        DeltaTime = _secondsPerTick,
                    });

                    _currentTick++;
                    _totalTicks++;

                    if (_currentTick >= _ticksPerSecond)
                    {
                        OnSecond?.Invoke(new SecondInfo());

                        _currentTick = 0;
                        _secondsElapsed++;
                    }
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
            _currentTick = 0;
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
                _secondsPerTick = 1f / _ticksPerSecond;
            }
        }
    }
}
