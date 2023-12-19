using System;
using UnityEngine;

namespace URMG.Core
{
public struct TickEventArgs
{
    public int Tick;
}
/// <summary>
/// Tick system.
/// </summary>
public sealed class TickSystem : MonoBehaviour
{
    public const int DefaultTPS = 20;
    public const int MaxTicksPerSecond = 10000;

    bool _isFrozen = false;
    public bool IsFrozen
    {
        get => _isFrozen;
    }

    int _ticksPerSecond = DefaultTPS;
    public int TicksPerSecond
    {
        get => _ticksPerSecond;
    }

    float _secondPerTick = 1 / DefaultTPS;
    public float SecondPerTick
    {
        get => _secondPerTick;
    }

    float _tickTimer;
    int _totalTicks;
    /// <summary>
    /// Total number of ticks.
    /// </summary>
    public int TotalTicks { get => _totalTicks; } 
    int _currentTick;
    /// <summary>
    /// The tick in the current second. A number between 0 and TicksPerSecond.
    /// </summary>
    public int CurrentTick { get => _currentTick; } 

    /// <summary>
    /// Fired every game tick.
    /// </summary>
    public event EventHandler<TickEventArgs> OnTick;
    /// <summary>
    /// Fired every real-time second.
    /// </summary>
    public event EventHandler<TickEventArgs> OnSecond;

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

    public void SetTPS(int value)
    {
        if (value < 0)
        {
            Debug.Log("[WARNING]: TPS cannot be a negative value.");
            return;
        }

        if (value == 0)
        {
            ToggleFreeze(true);
            return;
        }

        if (_isFrozen) ToggleFreeze(false);

        if (value > MaxTicksPerSecond)
        {
            _ticksPerSecond = MaxTicksPerSecond;
        } else
        {
            _ticksPerSecond = value;
        }
        _secondPerTick = 1 / _ticksPerSecond;
    }

    public void ToggleFreeze(bool value)
    {
        _isFrozen = value;
    }
}
}

