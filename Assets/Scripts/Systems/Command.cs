using System;
using System.Collections.Generic;

namespace UZSG
{
    public enum CommandPermissionLevel {
        Anybody, OperatorOnly, Administrator
    }

    public enum CommandLocationConstraint {
        Anywhere, MenuOnly, WorldOnly,
    }

    public struct CreateCommandOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Syntax { get; set; }
        public List<string> Aliases { get; set; }
        public CommandPermissionLevel PermissionLevel { get; set; }
        public CommandLocationConstraint LocationConstraint { get; set; }
        public List<EventHandler<string[]>> Callbacks { get; set; }
    }

    public class Command
    {
        public enum ArgumentType { Required, Optional }

        public struct Argument
        {
            public string Key;
            public ArgumentType Type;
        }

        /// <summary>
        /// Command name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Syntax required for proper execution.
        /// </summary>
        public string Syntax { get; set; }
        /// <summary>
        /// Command description.
        /// </summary>
        public string Description { get; set; }
        public List<string> Aliases { get; set; }
        public bool IsDebugCommand { get; internal set; } = false;
        /// <summary>
        /// The messages that follows the input.
        /// </summary>
        public List<Argument> Arguments = new();
        public CommandPermissionLevel PermissionLevel { get; set;}
        public CommandLocationConstraint LocationConstraint { get; set; }
        public List<EventHandler<string[]>> Callbacks = new();
        public event EventHandler<string[]> OnInvoke;
        
        public void Invoke(string[] args)
        {
            OnInvoke?.Invoke(this, args);
        }
    }
}