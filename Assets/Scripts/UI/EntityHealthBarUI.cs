using UnityEngine;

using TMPro;

using UZSG.Attributes;

namespace UZSG.UI
{
    public class EntityHealthBarUI : UIElement
    {
        public int Level
        {
            set => levelTmp.text = value.ToString();
        }
        [SerializeField] TextMeshProUGUI levelTmp;
        [SerializeField] AttributeBar attributeBar;

        public virtual void BindAttribute(Attribute attr)
        {
            attributeBar.BindAttribute(attr);
        }

        protected override void OnShow()
        {
            /// TODO: fade in?
        }

        protected override void OnHide()
        {
            /// TODO: fade out?
        }
    }
}