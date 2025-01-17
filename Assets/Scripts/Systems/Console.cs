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
        [SerializeField] bool EnableDebugMode = true;
        [SerializeField] bool enableDebugCommands = true;
        /// <summary>
        /// <c>string</c> is command Name.
        /// </summary>
        Dictionary<string, Command> _commandsDict = new();
        List<string> _messages = new();
        public List<string> Messages => _messages;
        ConsoleWindow gui;
        public ConsoleWindow Gui => gui;
        InputAction toggleUI;
        Player localPlayer;

        #region Events
        /// <summary>
        /// Called everytime the Console logs a message.
        /// </summary>
        public event Action<string> OnLogMessage;
        public event Action<Command> OnInvokeCommand;

        #endregion

        
        internal void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            Game.Main.OnLateInit += OnLateInit;
            LogInfo("Initializing console...");
            InitializeCommands();
        }

        void InitializeInputs()
        {
            toggleUI = Game.Main.GetInputAction("Hide/Show", "Console Window");
            toggleUI.performed += OnInputToggleUI;
            toggleUI.Enable();
        }


        void InitializeWorldEvents()
        {
            Game.World.CurrentWorld.OnPause += () =>
            {
                toggleUI.Disable();
            };
            Game.World.CurrentWorld.OnUnpause += () =>
            {
                toggleUI.Enable();
            };
        }


        #region Event callbacks

        void OnLateInit()
        {
            gui = Game.UI.Create<ConsoleWindow>("Console Window");
            InitializeInputs();
            
            Game.World.OnDoneLoadWorld += InitializeWorldEvents;
            // Game.World.OnExitWorld += DeinitializeWorldEvents;
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
        }
        
        void OnEntitySpawned(EntityManager.EntityInfo info)
        {
            if (info.Entity is Player player)
            {
                Game.Entity.OnEntitySpawned -= OnEntitySpawned;
                localPlayer = player;
            }
        }

        void OnInputToggleUI(InputAction.CallbackContext context)
        {
            if (gui.IsVisible)
                gui.Hide();
            else
                gui.Show();
        }

        #endregion

        /// <summary>
        /// Run a console command.
        /// </summary>
        public void Run(string input)
        {
            if (input.StartsWith("/")) input = input[1..]; /// Removes '/' if present
            string[] args = input.Split(' ');

            ExecuteCommand(args[0], args[1..]);
        }

        void ExecuteCommand(string command, string[] args)
        {
            if (command.StartsWith("/")) command = command[1..]; /// Removes '/' if present

            // if (Game.World.HasWorld)
            // if (Game.World.CurrentWorld.Attributes.OwnerId != commandSenderId)
            // {
            //     return;
            // }

            if (_commandsDict.TryGetValue(command, out var c))
            {
                try
                {
                    if (c.IsDebugCommand && !enableDebugCommands) return;

                    _commandsDict[command].Invoke(args);
                    return;
                }
                catch (Exception ex)
                {
                    LogError($"An internal exception occurred when performing the command");
                    Debug.LogException(ex);
                    return;
                }
            }
            else
            {
                PromptInvalid();
                return;
            }
        }

        /// <summary>
        /// Prompts invalid command usage.
        /// </summary>
        void PromptInvalid()
        {
            LogInfo("Invalid command. Type '/help' for a list of available commands.");
        }

        void PromptInvalid(string command)
        {
            if (_commandsDict.ContainsKey(command))
            {
                LogInfo($"Invalid command usage. Try '/help {command}'");
            }
            else
            {
                PromptInvalid();
            }
        }


        #region Public methods

        public void Write(string message)
        {
            _messages.Add($"{message}");
            OnLogMessage?.Invoke(message);
        }

        public void WriteLine(string message)
        {
            _messages.Add($"\n{message}");
            OnLogMessage?.Invoke($"\n{message}");
        }

        public void LogInfo(object message)
        {
            try
            {
                WriteLine($"{message}");
            }
            catch
            {
                UnityEngine.Debug.Log(message);
            }
        }
                
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogDebug(object message)
        {
            if (EnableDebugMode)
            {
                LogInfo($"<color=\"white\">[DEBUG]: {message}</color>");
            }
        }
        
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogWarn(object message)
        {
            LogInfo($"<color=\"orange\">[WARN]: {message}</color>");
        }

        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogError(object message)
        {
            LogInfo($"<color=\"red\">[ERROR]: {message}</color>");
        }

        /// <summary>
        /// Logs a message both to the UZSG console and Unity console.
        /// </summary>
        /// <param name="message"></param>
        public void LogWithUnity(object message)
        {
            Game.Console.LogWarn(message);
            UnityEngine.Debug.LogWarning(message);
        }

        #endregion
    }
}
