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

        public Item CraftItem(Item item)
        {
            RecipeData recipes = Game.Recipes.GetRecipeData(item.Id);

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
                    return Item.None;
                }
            }

            /// Takes item in the inventory
            /// NOTE: ENSURE THAT THE AVAILABILITY OF MATERIALS ARE FULLY CHECKED
            foreach (Item material in recipes.Materials)
            {
                int remainingCount = material.Count;

                foreach (ItemSlot slot in player.Inventory.Bag.Slots)
                {

                    if (slot.IsEmpty || material.CompareTo(slot.Item))
                    {
                        continue;
                    }

                    int takes = slot.Item.Count - remainingCount;

                    if (takes < remainingCount)
                    {
                        remainingCount -= slot.Item.Count;
                        slot.TakeAll();
                    }
                    else
                    {
                        remainingCount -= takes;
                        slot.TakeItems(takes);
                    }

                    if (remainingCount <= 0)
                    {
                        continue;
                    }
                }
            }

            return recipes.Output;
        }
    }
}