using System;
using System.Collections.Generic;
using UZSG.Entities;
using UZSG.Inventory;

namespace UZSG.Crafting 
{
    public class PlayerCrafting : InventoryCrafting
    {
        public Player _playerEntity;
        public Container OutputContainer = new(5);
        public List<CraftingRoutine> PlayerCraftingList = new();

        void Start(){
            _playerEntity = transform.parent.gameObject.GetComponent<Player>();
            InputContainer = _playerEntity.Inventory.Bag;
        }
        public void InitializePlayer(Player _player)
        {
            _playerEntity = _player;
        }
    }
}