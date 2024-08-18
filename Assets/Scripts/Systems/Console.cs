using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.UI;

namespace UZSG.Systems
{
    public sealed partial class Console : MonoBehaviour
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        public bool EnableDebugMode = true;
        /// <summary>
        /// Key is command name.
        /// </summary>
        Dictionary<string, Command> _commandsDict = new();
        public List<string> Messages;

        ConsoleWindow UI;
        

        #region Events
        /// <summary>
        /// Called everytime the Console logs a message.
        /// </summary>
        public event Action<string> OnLogMessage;
        public event Action<Command> OnInvokeCommand;

        #endregion


        InputAction toggleUI;
        Player _player;
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            Game.Main.OnLateInit += OnLateInit;

            Log("Initializing console...");
            InitializeCommands();
        }

        void OnLateInit()
        {
            UI = Game.UI.Create<ConsoleWindow>("Console Window");
            InitializeInputs();

            Game.Entity.OnEntitySpawned += (info) =>
            {
                if (info.Entity is Player player)
                {
                    _player = player;
                    UI.OnOpen += () =>
                    {
                        _player.Controls.Disable();
                        _player.Actions.Disable();
                        _player.FPP.ToggleControls(false);
                    };
                    UI.OnClose += () =>
                    {
                        _player.Controls.Enable();
                        _player.Actions.Enable();
                        _player.FPP.ToggleControls(true);
                    };
                }
            };
        }

        void InitializeInputs()
        {
            toggleUI = Game.Main.GetInputAction("Hide/Show", "Console Window");
            toggleUI.performed += OnInputToggleUI;
            toggleUI.Enable();
        }

        void OnInputToggleUI(InputAction.CallbackContext context)
        {
            UI.ToggleVisibility();
        }

        public void Run(string input)
        {
            if (input.StartsWith("/")) input = input[1..]; /// Remove '/' if present
            string[] split = input.Split(' ');

            ExecuteCommand(split[0], split[1..]);
        }

        void ExecuteCommand(string command, string[] args)
        {
            if (command[0] == '/')
            {
                command = command.Replace("/", "");
            }

            // try
            // {
                _commandsDict[command].Invoke(args);
            // }
            // catch 
            // {
            //     PromptInvalid(command);
            //     return;
            // }
        }

        /// <summary>
        /// Prompts invalid command usage.
        /// </summary>
        void PromptInvalid()
        {
            Log("Invalid command. Type '/help' for a list of available commands.");
        }

        void PromptInvalid(string command)
        {
            if (_commandsDict.ContainsKey(command))
            {
                Log($"Invalid command usage. Try '/help {command}'");
            }
            else
            {
                PromptInvalid();
            }
        }


        #region Public methods

        public void Write(string message)
        {
            Messages.Add($"{message}");
            OnLogMessage?.Invoke(message);
        }

        public void WriteLine(string message)
        {
            Messages.Add($"\n{message}");
            OnLogMessage?.Invoke($"\n{message}");
        }

        public void Log(object message)
        {
            try
            {
                WriteLine($"{message}");
            }
            catch
            {
                Debug.Log(message);
            }
        }
                
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogDebug(object message)
        {
            Log($"[DEBUG]: {message}");
        }
        
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogWarning(object message)
        {
            Log($"<color=\"orange\">[WARN]: {message}</color>");
        }

        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogError(object message)
        {
            Log($"<color=\"red\">[ERR]: {message}</color>");
        }

        public void LogAndUnityLog(object message)
        {
            Game.Console.LogWarning(message);
            Debug.LogWarning(message);
        }

        #endregion
    }
}
