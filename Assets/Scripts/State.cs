using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace URMG
{
    public abstract class State
    {
        public string Name;
        public int AnimId;
        public bool IsLocked;

        public State(string name)
        {
            Name = name;
        }

        public void Lock(bool value)
        {
            IsLocked = value;
        }

        /// <summary>
        /// Called once upon entering this state.
        /// </summary>
        public abstract void Enter();

        /// <summary>
        /// Called once every frame while in this state.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Called once upon exiting this state.
        /// </summary>
        public abstract void Exit();
    }
}