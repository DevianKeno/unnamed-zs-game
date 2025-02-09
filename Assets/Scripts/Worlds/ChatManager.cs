using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UZSG.Entities;
using UZSG.UI;

namespace UZSG.Worlds
{
    /// <summary>
    /// Handles the chat system when within worlds.
    /// </summary>
    public class ChatManager : MonoBehaviour
    {
        public World World { get; private set; }
        
        bool _isInitialized;
        Player localPlayer;

        public event Action<string> OnSendMessage;

        InputAction toggleInput;
        ChatManagerUI ui;
        public ChatManagerUI UI => ui;

        void Awake()
        {
            World = GetComponentInParent<World>();
        }

        internal void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;
            ui = Game.UI.Create<ChatManagerUI>("Chat Manager (UI)");
            ui.Initialize(this);
            ui.SetInactive();

            toggleInput = Game.Main.GetInputAction("Open Chat", "Player");
            toggleInput.performed += OnKeyDownEnter;

            Game.UI.OnAnyWindowOpened += OnAnyWindowOpened;
        }

        void OnDestroy()
        {
            Game.UI.OnAnyWindowOpened -= OnAnyWindowOpened;
            ui.Destruct();
        }

        void OnKeyDownEnter(InputAction.CallbackContext context)
        {
            if (Game.UI.HasActiveWindow) return;

            if (ui.IsActive && !ui.HasInput) /// only close if no inputs
            {
                ui.SetInactive();
                
                var player = World.GetLocalPlayer();
                player.Actions.Enable();
                player.Controls.Enable();
            }
            else
            {
                ui.SetActive();

                var player = World.GetLocalPlayer();
                player.Actions.Disable();
                player.Controls.Disable();
            }
        }

        void OnAnyWindowOpened(Window window)
        {
            if (ui.IsActive)
            {
                ui.SetInactive();
            }
        }

        public void SendMessageRaw(object message)
        {
            /// Try to invoke command if it's one
            if (message is string str && str.StartsWith(Console.COMMAND_PREFIX))
            {
                Game.Console.RunCommand(str);
                return;
            }
            else /// just a chat message
            {
                ui.AddMessage($"{message}\n");
            }
            OnSendMessage?.Invoke(message.ToString());
        }

        public void SendMessage(object message)
        {
            /// Try to invoke command if it's one
            if (message is string str && str.StartsWith(Console.COMMAND_PREFIX))
            {
                Game.Console.RunCommand(str);
                return;
            }
            else /// just a chat message
            {
                ui.AddMessage($"{World.GetLocalPlayer().DisplayName ?? "Player"}: " + message.ToString() + '\n');
            }
            OnSendMessage?.Invoke(message.ToString());
        }
    }
}