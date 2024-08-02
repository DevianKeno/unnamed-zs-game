using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Crafting;
using UZSG.Items;
using UZSG.Systems;
using UnityEngine.EventSystems;

namespace UZSG.UI
{
    public class CraftableItemUI : Window
    {
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI text;

        public void SetItem(ItemData data)
        {
            image.sprite = data.Sprite;
            text.text = data.Name;
        }
    }
}