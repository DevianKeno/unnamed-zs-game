using UnityEngine;

namespace UZSG.UI
{
    /// <summary>
    /// GUIs that can be appended to the Player's Inventory GUI.
    /// </summary>
    public interface IInventoryWindowAppendable
    {
        public GameObject gameObject { get; }
        public Frame Frame { get; }
    }
}