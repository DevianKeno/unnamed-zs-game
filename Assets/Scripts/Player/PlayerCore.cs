using System;
using UnityEngine;
using UZSG.Systems;
using UZSG.Inventory;

namespace UZSG.Player
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