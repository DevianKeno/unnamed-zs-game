using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class Selector : MonoBehaviour
    {
        public RectTransform rect;
        Image image;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        public void Select(ItemSlotUI element)
        {
            rect.position = element.rect.position;
            rect.sizeDelta = element.rect.sizeDelta;
            image.enabled = true;
        }

        public void Show()
        {
            image.enabled = true;
        }

        public void Hide()
        {
            image.enabled = false;
        }
    }
}
