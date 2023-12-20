using UnityEngine;
using UnityEngine.UI;
using TMPro;
using URMG.Items;

namespace URMG.UI
{
    public class ItemDisplayUI : MonoBehaviour
    {
        public Item Item;
        [SerializeField] Image image;
        [SerializeField] TextMeshProUGUI countText;
        [SerializeField] TextMeshProUGUI altText;

        void Start()
        {            
            image.color = SlotUI.Transparent;
        }
        
        public void SetDisplay(Item item)
        {
            if (item == Item.None)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            if (item.Data.Sprite != null)
            {
                altText.enabled = false;
                image.sprite = item.Data.Sprite;
                image.color = SlotUI.Opaque;
            } else
            {
                altText.enabled = true;
                altText.text = item.Data.Name;
                image.color = SlotUI.Transparent;
            }

            if (item.Count == 1)
            {
                countText.text = "";
            } else
            {
                countText.text = item.Count.ToString();
            }
        }
    }
}