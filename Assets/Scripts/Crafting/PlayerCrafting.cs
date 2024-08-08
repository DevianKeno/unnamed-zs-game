using System;
using UZSG.Entities;
using UZSG.Inventory;

namespace UZSG.Crafting 
{
    public class PlayerCrafting : InventoryCrafting
    {
        public Player _playerEntity;
        public Container PlayerInventory;
        public Container OutputContainer = new(5);

        void Start(){
            _playerEntity = this.transform.parent.gameObject.GetComponent<Player>();
            PlayerInventory = _playerEntity.Inventory.Bag;
        }
        public void InitializePlayer(Player _player)
        {
            _playerEntity = _player;
        }
    }
}