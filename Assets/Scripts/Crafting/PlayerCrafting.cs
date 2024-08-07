using System;
using UZSG.Entities;
using UZSG.Inventory;

namespace UZSG.Crafting 
{
    public class PlayerCrafting : InventoryCrafting
    {
        public Player _playerEntity;
        public void InitializePlayer(Player _player)
        {
            _playerEntity = _player;
        }
    }
}