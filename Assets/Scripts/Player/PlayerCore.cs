using System;
using UnityEngine;
using URMG.Core;
using URMG.InventoryS;

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
        
        [SerializeField] InventoryHandler _inventory;
        public InventoryHandler Inventory { get => _inventory; }

    }
}