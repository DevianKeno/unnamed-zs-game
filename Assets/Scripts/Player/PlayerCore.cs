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
        
        [SerializeField] BagHandler _inventory;
        public BagHandler Inventory { get => _inventory; }
    }
}