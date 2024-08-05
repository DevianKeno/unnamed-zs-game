using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UZSG.Systems
{
    public struct CommandInvokedArgs
    {

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
        /// <summary>
        /// The messages that follows the input.
        /// </summary>
        public List<Argument> Arguments = new();
        public event EventHandler<string[]> OnInvoke;
        
        public void Invoke(string[] args)
        {
            OnInvoke?.Invoke(this, args);
        }
    }
}