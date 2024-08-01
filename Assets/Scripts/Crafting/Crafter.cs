using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;
using UZSG.Systems;

namespace UZSG.Crafting
{
    public class Crafter : MonoBehaviour
    {
        // public Item Output;
        public int Amount;
        public Player player;

        public void ViewRecipe(Item item)
        {
            RecipeData recipes = Game.Recipes.GetRecipeData(item.Id);

            foreach (Item material in recipes.Materials)
            {
                print(material.Name);
            }
        }

        public void testCommand(string testText)
        {
            print(testText);
        }

        public void CraftItem(Item item)
        {
            RecipeData recipes = Game.Recipes.GetRecipeData(item.Id);

            var _availableSlot = new List<ItemSlot>(); 

            /// Check if sufficient materials is available inside the player inventory
            foreach (Item material in recipes.Materials)
            {
                int count = 0;

                if (player.Inventory.Bag.ContainsCount(item: material, out var slot))
                {
                    count += slot.Item.Count;
                }

                if (count < material.Count)
                {
                    print("Materials Required does not match players current Inventory");
                    return;
                }
            }

            /// Takes item in the inventory
            /// NOTE: ENSURE THAT THE AVAILABILITY OF MATERIALS ARE FULLY CHECKED
            foreach (Item material in recipes.Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in player.Inventory.Bag.Slots)
                {
                    if (slot.IsEmpty)
                    {
                        continue;
                    }

                    if (!material.CompareTo(slot.Item)){
                        continue;
                    }
                    /*
                        comparator checks the difference of the remaining count and the
                        required count of the material.
                    */
                    int comparator = slot.Item.Count - remainingCount;

                    if (comparator > 0)
                    {
                        remainingCount -= material.Count;
                        slot.TakeItems(material.Count);
                    }
                    else 
                    {
                        remainingCount -= slot.Item.Count;
                        slot.TakeAll();
                    }

                    if (remainingCount <= 0)
                    {
                        continue;
                    }
                }
            }

            Item _newItem = new Item(recipes.Output);

            player.Inventory.Bag.TryPutNearest(_newItem);
        }
    }
}