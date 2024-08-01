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
            var dictSlots = new Dictionary<Item, List<ItemSlot>>();

            /// Check if sufficient materials is available inside the player inventory
            foreach (Item material in recipes.Materials)
            {
                var materialSlots = new List<ItemSlot>();

                if (player.Inventory.Bag.ContainsCount(item: material, material.Count, out materialSlots))
                {
                    dictSlots.Add(material, materialSlots);
                } else {
                    print("Materials Required does not match players current Inventory");
                    return;
                }
            }
            /// Takes item in the inventory
            /// NOTE: ENSURE THAT THE AVAILABILITY OF MATERIALS ARE FULLY CHECKED
            foreach (Item material in recipes.Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in dictSlots[material])
                {
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