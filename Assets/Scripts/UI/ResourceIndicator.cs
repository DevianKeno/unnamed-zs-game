using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UZSG.Interactions;
using UZSG.Entities;

namespace UZSG.UI
{
    public class ResourceIndicator : Window
    {
        public string Text
        {
            get
            {
                return resourceText.text;
            }
            set
            {
                resourceText.text = value;
            }
        }
        public Sprite Icon
        {
            get
            {
                return iconImage.sprite;
            }
            set
            {
                iconImage.sprite = value;
            }
        }

        [SerializeField] Image iconImage;
        [SerializeField] TextMeshProUGUI resourceText;
    }
}
