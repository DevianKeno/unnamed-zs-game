using UnityEngine;
using TMPro;

using UZSG.Data;
using UZSG.Items;

namespace UZSG.UI
{
    public class CraftedItemDisplayUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] ItemDisplayUI itemDisplay;
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI descriptionText;

        public void SetDisplayedRecipe(RecipeData data)
        {
            ResetDisplayed();

            itemDisplay.SetDisplayedItem(data.Output);
            itemNameText.text = data.Output.Data.Name;
            descriptionText.text = data.Output.Data.Description;
        }

        public void ResetDisplayed()
        {
            itemDisplay.SetDisplayedItem(Item.None);
            itemNameText.text = "Select recipe";
            descriptionText.text = "";
        }
    }
}