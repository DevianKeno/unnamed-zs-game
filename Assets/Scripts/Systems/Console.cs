using System.Collections.Generic;
using UnityEngine;
using URMG.Systems;

namespace URMG.Console
{
    public readonly struct Command
    {
        readonly string _name; 
        public readonly string Name { get => _name; } 
        readonly string _description; 
        public readonly string Description { get => _description; } 

        public Command(string name, string description)
        {
            _name = name;
            _description = description;
        }
    }

    public struct CommandArgs
    {
        public string[] args;

        public readonly string this[int i]
        {
            get => args[i];
        }
    }

    public class Console : MonoBehaviour
    {
        List<Command> _commands = new();
        public string Input;

        void Awake()
        {
            Game.Main.OnInitialize += Init;
        }

        void Init()
        {
            
        }

        public void SubmitInput()
        {
            CommandArgs args = new CommandArgs();
        }

        public void RunCommand(Command command, CommandArgs args)
        {

        }

        public void Log(object obj)
        {
            Debug.Log(obj);
        }
    }

    public class GUI
    {

    }
}
