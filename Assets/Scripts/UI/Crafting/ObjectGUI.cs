using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Entities;
using UZSG.Objects;
using UZSG.UI.Players;

namespace UZSG.UI.Objects
{
    /// <summary>
    /// GUI class for Objects that have GUIs.
    /// </summary>
    public abstract class ObjectGUI : UIElement, IInventoryAppendable
    {
        protected BaseObject baseObject;
        /// <summary>
        /// The object tied to this GUI.
        /// </summary>
        public BaseObject BaseObject => baseObject;
        protected Player player;
        /// <summary>
        /// The Player who's interacting with this GUI.
        /// </summary>
        public Player Player => player;

        [SerializeField] Frame frame;
        public Frame Frame => frame;
        
        InputAction backAction;
        
        public virtual void SetPlayer(Player player)
        {
            this.player = player;
            backAction = Game.Main.GetInputAction("Back", "Global");
        }

        protected override void OnShow()
        {
            backAction.performed += OnInputGlobalBack;
        }

        protected override void OnHide()
        {
            backAction.performed -= OnInputGlobalBack;
        }

        protected virtual void OnInputGlobalBack(InputAction.CallbackContext context)
        {
            Hide();
        }
    }
}