using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Items;

namespace UZSG.UI
{
    /// <summary>
    /// UI element for displaying Items (with count).
    /// </summary>
    public class ItemDisplayUI : Window, IUIElement
    {
        [SerializeField] Item item = Item.None;
        public Item Item
        {
            get
            {
                return item;
            }
            set
            {
                SetDisplayedItem(item);
            }
        }
        public string Count
        {
            get
            {
                return item.Count.ToString();
            }
            set
            {
                countText.text = value;
            }
        }

        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI countText;
        [SerializeField] TextMeshProUGUI altText;

        void OnEnable()
        {
            image.preserveAspect = true;
        }
        
        public void SetDisplayedItem(Item item)
        {            
            this.item = item;
            
            if (item.IsNone)
            {
                image.sprite = null;
                image.color = ItemSlotUI.Transparent;
                countText.text = "";
                altText.enabled = false;
            }
            else
            {
                image.sprite = item.Data.Sprite;
                if (item.Data.Sprite != null)
                {
                    image.color = ItemSlotUI.Opaque;
                    altText.text = "";
                    altText.enabled = false;
                }
                else
                {
                    image.color = ItemSlotUI.Transparent;
                    altText.enabled = true;
                    altText.text = item.Data.Name;
                }

                if (item.Count <= 1)
                {
                    countText.text = "";
                }
                else
                {
                    countText.text = item.Count.ToString();
                }
            }
        }

        public void DisplayCount(bool display)
        {
            if (display)
            {
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }
    }
}