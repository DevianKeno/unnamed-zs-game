using UnityEngine;
using UZSG.Attributes;

namespace UZSG.UI
{
    public class AttributeBar : ProgressBar
    {
        [SerializeField] protected Attribute attribute;
        public Attribute Attribute => attribute;

        public void BindAttribute(Attribute value)
        {
            if (!value.IsValid) return;

            attribute = value;
            attribute.OnValueChanged += (sender, e) =>
            {
                Value = attribute.ValueMaxRatio;
            };

            Refresh();
        }
    }
}