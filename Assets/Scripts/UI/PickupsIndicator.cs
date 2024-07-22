using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Items;

namespace UZSG.UI
{
    public class PickupsIndicator : MonoBehaviour, IUIElement
    {
        [SerializeField] Image itemImage;
        [SerializeField] TextMeshProUGUI itemNameTMP;
        [SerializeField] TextMeshProUGUI countTMP;

        public void SetDisplayedItem(Item item)
        {
            if (item == null) return;
            if (item.Data == null) return;

            itemImage.sprite = item.Data.Sprite;
            itemNameTMP.text = item.Data.Name;
            countTMP.text = item.Count.ToString();
        }

        public void PlayAnimation()
        {
            Destroy(gameObject, 3f);
        }
    }
}