using System;
using System.Collections.Generic;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;

namespace UZSG.Crafting 
{
    public class PlayerCrafting : InventoryCrafting
    {
        public Player _playerEntity;
        public Container OutputContainer = new(5);
        public Container InputContainer;
        public List<CraftingRoutine> PlayerCraftingList = new();

        void Start(){
            _playerEntity = transform.parent.gameObject.GetComponent<Player>();
            InputContainer = _playerEntity.Inventory.Bag;
            OutputContainer = _playerEntity.Inventory.Bag; //For testing purposes only. delete this in production
        }
        public void InitializePlayer(Player _player)
        {
            _playerEntity = _player;
        }

        public void PlayerCraftItem(RecipeData recipe){
            CraftItem(recipe, InputContainer, OutputContainer, PlayerCraftingList);
        }
    }
}