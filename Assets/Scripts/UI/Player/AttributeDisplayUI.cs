using UnityEngine;
using TMPro;

namespace UZSG.UI
{
    public class AttributeDisplayUI : Window
    {
        [SerializeField]  Attributes.Attribute attribute;
        public Attributes.Attribute Attribute
        {
            get
            {
                return attribute;
            }
            set
            {
                if (!attribute.IsValid) return;

                attribute = value;
                Refresh();
            }
        }

        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] AttributeBar bar;

        public void Refresh()
        {
            nameText.text = attribute.Data.Name;
            valueText.text = $"{attribute.Value}";

            bar.Value = attribute.ValueMaxRatio;
        }
    }
}