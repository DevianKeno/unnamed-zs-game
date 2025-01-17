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
    public abstract class ObjectGUI : UIElement, IInventoryWindowAppendable
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
                
        public virtual void SetPlayer(Player player)
        {
            this.player = player;
        }
    }
}