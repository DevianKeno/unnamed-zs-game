using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Entities;
using UZSG.EOS;
using UZSG.UI;
using UZSG.UI.Players;

namespace UZSG.Systems
{
    public enum LogMessageType {
        Info, Warn, Error, Fatal
    }

    public class InvalidCommandUsageException : Exception
    {
        
    }

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

        public void StartListenForLocalPlayer()
        {
            Game.Entity.OnEntitySpawned += OnEntitySpawned;
            _creativeIsOn = false; /// TODO: save to player
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
            if (splitAt >= 0)
            {
                var command = input[..splitAt];
                var args = input [(splitAt + 1)..];

                ExecuteCommand(command, args);
            }
            else
            {
                ExecuteCommand(input, string.Empty);
            }

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
                if (c.LocationConstraint == CommandLocationConstraint.WorldOnly && !Game.World.IsInWorld)
                {
                    Game.Console.LogInfo($"The command '{command} can only be executed within a world.");
                    return;
                }
                if (c.IsDebugCommand && !enableDebugCommands) 
                {
                    Game.Console.LogInfo($"The command '{command} is a debug command. Enable debugging first.");
                    return;
                }

                try
                {
                    if (!NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer)
                    {
                        _commandsDict[command].Invoke(args.Split(' '));
                    }
                    else
                    {
                        // EOSSubManagers.P2P.RequestCommandInvoke(EOSSubManagers.Transport.GetEOSTransport().ServerUserId, command, args);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    if (ex is InvalidCommandUsageException)
                    {
                        PromptInvalid(command);
                        return;
                    }
                    else
                    {
                        LogError($"An internal exception occurred when performing the command");
                        Debug.LogException(ex);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Prompts invalid command usage.
        /// </summary>
        void PromptInvalid()
        {
            LogInfo("Invalid command. Type '/help' for a list of available commands.");
        }

        /// <summary>
        /// Prompts invalid command usage.
        /// </summary>
        void PromptInvalid(string command)
        {
            LogInfo($"Invalid command usage. Try '/help {command}'");
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
                WriteLine($"{message.ToString()}");
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

        public void LogFormat(string format, params object[] args)
        {
            Debug.LogFormat(format, args);
        }

        public void Log(object message, LogMessageType type = LogMessageType.Info)
        {
            switch (type)
            {
                case LogMessageType.Info:
                    LogInfo(message);
                    break;
                case LogMessageType.Warn:
                    LogWarn(message);
                    break;
                case LogMessageType.Error:
                    LogError(message);
                    break;
                case LogMessageType.Fatal:
                    LogFatal(message);
                    break;
            }
        }

        /// <summary>
        /// Asserts a condition, logging an error message upon failure.
        /// </summary>
        public void Assert(bool value, object message = null)
        {
            if (!value) LogError(message);
            Debug.Assert(value);
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
