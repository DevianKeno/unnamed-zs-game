using UZSG.Entities;
using UZSG.Objects;

namespace UZSG.UI
{
    /// <summary>
    /// GUI class for Objects that have GUIs.
    /// </summary>
    public abstract class ObjectGUI : UIElement, IInventoryWindowAppendable
    {
        /// <summary>
        /// The object tied to this GUI.
        /// </summary>
        public BaseObject BaseObject { get; set; }
        /// <summary>
        /// The Player who's interacting with this GUI.
        /// </summary>
        public Player Player { get; set; }
        public Frame Frame { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            Frame = GetComponent<Frame>();
        }

        public virtual void SetPlayer(Player player)
        {
            this.Player = player;
        }
    }
}