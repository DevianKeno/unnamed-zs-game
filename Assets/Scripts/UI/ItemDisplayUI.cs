using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Items;

namespace UZSG.UI
{
    public class ItemDisplayUI : MonoBehaviour, IUIElement
    {
        public Item Item;
        public bool IsVisible { get; set; }

        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI countText;
        [SerializeField] TextMeshProUGUI altText;


        void Start()
        {
            image.preserveAspect = true;
            image.color = ItemSlotUI.Transparent;

            if (Item != null)
            {
                SetDisplay(Item);
            }
        }
        
        public void SetDisplay(Item item)
        {
            Item = item;
            
            if (item != Item.None)
            {
                if (item.Data != null)
                {
                    if (item.Data.Sprite != null)
                    {
                        altText.enabled = false;
                        image.sprite = item.Data.Sprite;
                        image.color = ItemSlotUI.Opaque;
                    }
                    else
                    {
                        altText.enabled = true;
                        altText.text = item.Data.Name;
                        image.color = ItemSlotUI.Transparent;
                    }

                    if (item.Count <= 1)
                    {
                        countText.text = "";
                    }
                    else
                    {
                        countText.text = item.Count.ToString();
                    }

                    return;
                }
            }
            
            altText.enabled = false;
            image.sprite = null;
            image.color = ItemSlotUI.Transparent;
            countText.text = "";
        }

        public void ToggleVisibility()
        {
            throw new System.NotImplementedException();
        }

        public void SetVisible(bool visible)
        {
            throw new System.NotImplementedException();
        }
    }
}