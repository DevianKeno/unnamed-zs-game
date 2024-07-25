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
        }
        
        public void SetDisplayedItem(Item item)
        {            
            Item = item;
            
            if (item == null || item.IsNone)
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