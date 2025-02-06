using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UZSG.Items;


namespace UZSG.UI
{
    public class ItemDetailsUI : UIElement
    {
        [SerializeField] protected Item item;
        public Item Item
        {
            get
            {
                return item;
            }
            set
            {
                Refresh(value);
            }
        }

        List<AttributeDisplayUI> _attrDisplayUIs = new();

        [SerializeField] ItemDisplayUI itemDisplay;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] Transform attrDisplaysContainer;

        void Refresh(Item item)
        {
            if (this.item == item) return;

            this.item = item;
            itemDisplay.Item = item;
            ClearAttributeDisplays();

            if (item.IsNone)
            {
                nameText.text = "";
                descriptionText.text = "";
            }
            else
            {
                nameText.text = item.Data.DisplayName;
                descriptionText.text = item.Data.Description;
                
                if (item.Attributes != null)
                {
                    foreach (var attr in item.Attributes)
                    {
                        CreateAttributeDisplay(attr);
                    }
                }
            }
        }

        public void ClearAttributeDisplays()
        {
            foreach (Transform c in attrDisplaysContainer)
            {
                Destroy(c.gameObject);
            }
            _attrDisplayUIs.Clear();
        }

        void CreateAttributeDisplay(Attributes.Attribute attr)
        {
            var ui = Game.UI.Create<AttributeDisplayUI>("Attribute Display UI");
            ui.Attribute = attr;
        }
    }
}