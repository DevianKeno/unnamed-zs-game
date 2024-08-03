using System;
using System.Collections.Generic;

using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

using UZSG.Crafting;
using UZSG.Items;
using UZSG.Systems;
using UnityEngine.UI;
using UZSG.Data;

namespace UZSG.UI
{
    public class CraftedItemDisplayUI : MonoBehaviour
    {
        [SerializeField] ItemDisplayUI itemDisplay;
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI descriptionText;

        public void SetDisplayedItem(Item item)
        {
            itemDisplay.SetDisplayedItem(item);
            itemNameText.text = item.Data.Name;
            descriptionText.text = item.Data.Description;
        }

        public void ResetDisplayed()
        {
            itemDisplay.SetDisplayedItem(Item.None);
            itemNameText.text = "Select recipe";
            descriptionText.text = "";

        }
    }
}