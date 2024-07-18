using UnityEngine;
using UnityEngine.UI;

namespace UZSG.UI
{
    public class Selector : MonoBehaviour
    {
        [SerializeField] RectTransform rect;
        [SerializeField] Image image;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        public void Select(ItemSlotUI element)
        {
            var elementRect = element.GetComponent<RectTransform>();
            rect.position = elementRect.position;
            rect.sizeDelta = elementRect.sizeDelta;
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
