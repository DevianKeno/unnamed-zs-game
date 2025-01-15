using System;

using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Systems;

namespace UZSG.Systems
{
    /// <summary>
    /// Input manager for UZSG.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public InputAction InteractPrimary => Game.Main.GetInputAction("Interact", "Player Actions");
        public InputAction InteractSecondary => Game.Main.GetInputAction("Interact Secondary", "Player Actions");
    }
}