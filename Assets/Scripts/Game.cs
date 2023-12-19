using System;
using UnityEngine;
using URMG.UI;

namespace URMG.Core
{
public sealed class Game : MonoBehaviour
{
    public static Game Main { get; private set; }
    static UISystem _UI;
    public static UISystem UI { get => _UI; }
    static TickSystem _tick;
    public static TickSystem Tick { get => _tick; }
    static Console.Console _console;
    public static Console.Console Console { get => _console; }
    public event Action OnInitialize;

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (Main != null && Main != this)
        {
            Destroy(this);
        } else
        {
            Main = this;
            _UI = GetComponentInChildren<UISystem>();
            _tick = GetComponentInChildren<TickSystem>();
            _console = GetComponentInChildren<Console.Console>();                
        }

        Init();
    }

    void Init()
    {
        OnInitialize?.Invoke();

        UI.Init();
    }
}
}
