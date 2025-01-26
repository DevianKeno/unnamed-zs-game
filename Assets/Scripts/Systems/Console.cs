using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Entities;
using UZSG.UI;
using UZSG.UI.Players;

namespace UZSG.Systems
{
    public sealed partial class Console : MonoBehaviour
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
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
            Game.World.OnDoneLoadWorld -= OnDoneLoadWorld;
            Game.World.OnDoneLoadWorld += OnDoneLoadWorld;

            Game.World.CurrentWorld.OnPause -= OnWorldPaused;
            Game.World.CurrentWorld.OnPause += OnWorldPaused;

            Game.World.CurrentWorld.OnUnpause -= OnWorldUnpaused;
            Game.World.CurrentWorld.OnUnpause += OnWorldUnpaused;
        }

        void OnDoneLoadWorld()
        {
            if (_creativeIsOn)
            {
                creativeWindow = Game.UI.Create<CreativeWindow>("Creative Window");
                creativeWindow.Initialize(localPlayer);
                localPlayer.InventoryWindow.Append(creativeWindow);
            }
        }

        void OnWorldPaused()
        {
            toggleUI.Disable();
        }

        void OnWorldUnpaused()
        {
            toggleUI.Enable();
        }


        #region Event callbacks

        void OnLateInit()
        {
            gui = Game.UI.Create<ConsoleWindow>("Console Window");
            gui.Hide();
            InitializeInputs();
            
            Game.World.OnDoneLoadWorld += InitializeWorldEvents;
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
        public void RunCommand(string input)
        {
            if (string.IsNullOrEmpty(input)) return;
            if (input.StartsWith("/")) input = input[1..]; /// Removes '/' if present

            input = input.ToLower();
            int splitAt = input.IndexOf(' ');
            var command = input[..splitAt];
            var args = input [(splitAt + 1)..];

            ExecuteCommand(command, args);
        }

        void ExecuteCommand(string command, string args)
        {
            // if (Game.World.HasWorld)
            // if (Game.World.CurrentWorld.Attributes.OwnerId != commandSenderId)
            // {
            //     return;
            // }

            if (command.Equals("msg", StringComparison.OrdinalIgnoreCase))
            {
                var sliced = args.Split(' ', count: 2);
                CMessage(this, sliced[0], sliced[1]);
                return;
            }

            if (_commandsDict.TryGetValue(command, out var c))
            {
                try
                {
                    if (c.IsDebugCommand && !enableDebugCommands) return;

                    _commandsDict[command].Invoke(args.Split(' '));
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

        /// <summary>
        /// Log a message to the in-game console.
        /// </summary>
        public void LogInfo(object message)
        {
            if (message is string)
            {
                WriteLine($"{message}");
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }
                
        /// <summary>
        /// If debugging is enabled, logs a debug message into the in-game console as well as Unity console.
        /// </summary>
        public void LogDebug(object message, bool logWithUnity = true)
        {
            if (Game.Main.EnableDebugMode)
            {
                LogInfo($"<b><color=#B6F7FF>[DEBUG]:</b> {message}</color>");
                if (logWithUnity) Debug.Log(message);
            }
        }
        
        /// <summary>
        /// Log a yellow warning message into the game's console.
        /// </summary>
        public void LogWarn(object message)
        {
            LogInfo($"<b><color=\"yellow\">[WARN]:</b> {message}</color>");
        }

        /// <summary>
        /// Log an orange error message into the game's console.
        /// </summary>
        public void LogError(object message)
        {
            LogInfo($"<b><color=\"orange\">[ERROR]:</b> {message}</color>");
        }
        
        /// <summary>
        /// Log a red fatal message into the game's console.
        /// </summary>
        public void LogFatal(object message)
        {
            LogInfo($"<b><color=\"red\">[FATAL]:</b> {message}</color>");
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
