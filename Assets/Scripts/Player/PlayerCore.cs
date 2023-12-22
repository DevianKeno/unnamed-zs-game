using System;
using UnityEngine;
using URMG.Systems;
using URMG.Inventory;

namespace URMG.Player
{
    /// <summary>
    /// Player core functionalities.
    /// This contains all information related to the Player.
    /// </summary>
    public class PlayerCore : MonoBehaviour
    {
        public bool CanPickUpItems = true;
        bool _isSpawned = false;
        bool _isAlive = false;

        public int EquippedSlot;
        
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory { get => _inventory; }

        
    }
}