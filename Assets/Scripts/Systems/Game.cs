using System;
using UnityEngine;
using UZSG.UI;
using UZSG.World;

namespace UZSG.Systems
{
    public sealed class Game : MonoBehaviour
    {
        public static Game Main { get; private set; }
        static UISystem _UI;
        public static UISystem UI { get => _UI; }
        static TickSystem _tick;
        public static TickSystem Tick { get => _tick; }
        static WorldManager _worldManager;
        public static WorldManager World { get => _worldManager; }
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
                _worldManager = GetComponentInChildren<WorldManager>();   
                _console = GetComponentInChildren<Console.Console>();                 
            }

            Init();
        }

        void Init()
        {
            OnInitialize?.Invoke();
        }
    }
}
