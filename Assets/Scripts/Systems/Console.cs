using System;
using System.Collections.Generic;
using UnityEngine;
using UZSG.UI;

namespace UZSG.Systems
{
    public struct CommandArgs
    {
        public string[] Args;

        public readonly string this[int i]
        {
            get => Args[i];
        }

        public CommandArgs(string[] args)
        {
            Args = args;
        }
    }

    public sealed partial class Console : MonoBehaviour
    {
        bool _isInitialized;
        public bool IsInitialized => _isInitialized;
        public bool EnableDebugMode = true;
        Dictionary<string, Command> _commands = new();
        public List<string> Messages;

        /// <summary>
        /// Called everytime the Console logs a message.
        /// </summary>
        public event Action<string> OnLogMessage;
        public event Action<Command> OnInvokeCommand;

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            LogDebug("Initializing console...");
            InitializeCommands();
        }

        void InitializeCommands()
        {
            LogDebug("Initializing command registry...");
            /// Arguments enclosed in <> are required, [] are optional
            
            CreateCommand("clear",
                          "Clears the console messages.")
                          .OnInvoke += CClear;

            CreateCommand("freecam",
                          "Toggle Free Look Camera.")
                          .OnInvoke += CFreecam;
            
            CreateCommand("help",
                          "Prints help message.")
                          .OnInvoke += CHelp;
            
            CreateCommand("item <item_id> [amount]",
                          "Gives the player the item.")
                          .OnInvoke += CItem;

            CreateCommand("say <message>",
                          "Send a message.")
                          .OnInvoke += CSay;
            
            CreateCommand("spawn <entity_id> [x] [y] [z]",
                          "Spawns an entity.")
                          .OnInvoke += CSpawn;

            CreateCommand("teleport <x> <y> <z>",
                          "Teleport to coordinates.")
                          .OnInvoke += CTeleport;

            CreateCommand("tick <set|freeze> [value]",
                          "Manipulate in-game tick system.")
                          .OnInvoke += CTick;

            CreateCommand("world <create|load> <world_name>",
                          "")
                          .OnInvoke += CWorld;

            // /spawn item "bandage" 1

            // CreateCommand("tick <freeze|set> <value>",
            //               "Control the game's tick rate.").AddCallback(Command_Tick);
        }

        /// <summary>
        /// Create a new Console command.
        /// </summary>
        Command CreateCommand(string command, string description = "")
        {
            string[] args = command.Split(" ");

            Command newCommand = new()
            {
                Name = args[0],
                Syntax = command,
                Description = description
            };

            string substr;
            foreach (string arg in args[1..])
            {
                if (arg.Contains("|"))
                {
                    // Represents AND arguments
                    // Currently unhandled logic
                    string[] split = arg.Split("|");
                }

                if (arg.StartsWith("<") && arg.EndsWith(">")) // Required argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Required
                    });

                } else if (arg.StartsWith("[") && arg.EndsWith("]")) // Optional argument
                {
                    substr = arg[1..^1];
                    newCommand.Arguments.Add(new()
                    {
                        Type = Command.ArgumentType.Optional
                    });
                }
            }

            _commands[args[0]] = newCommand; 
            return newCommand;           
        }

        public void Run(string input)
        {
            if (input.StartsWith("/")) input = input[1..]; // Remove '/' if present
            string[] split = input.Split(' ');

            RunCommand(split[0], split[1..]);
        }

        void RunCommand(string command, string[] args)
        {
            if (_commands.ContainsKey(command))
            {
                // Invalid command arguments are currently unhandled
                _commands[command].Invoke(args);
            } else
            {
                PromptInvalid();
            }
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
            if (_commands.ContainsKey(command))
            {
                Log($"Invalid command usage. Try '/help {command}'");
            } else
            {
                PromptInvalid();
            }
        }

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
            } catch
            {
                Debug.Log(message);
            }
        }
        
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogDebug(object message)
        {
            if (!EnableDebugMode) return;
            Log(message);
        }
        
        /// <summary>
        /// Log a debug message into the game's console.
        /// </summary>
        public void LogWarning(object message)
        {
            if (!EnableDebugMode) return;
            Log($"[WARNING]: {message}");
        }
    }
}
