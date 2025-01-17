using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Items;
using UZSG.Attributes;
using System;

namespace UZSG.UI
{
    /// <summary>
    /// UI element for displaying Items (with count).
    /// </summary>
    public class ItemDisplayUI : UIElement
    {
        [SerializeField] Item item = Item.None;
        public Item Item
        {
            get => item;
            set
            {
                SetDisplayedItem(item);
            }
        }
        public int Count
        {
            get => item.Count;
            set
            {
                countText.text = value.ToString();
            }
        }

        #region Listening to attribute/s
        UZSG.Attributes.Attribute durability;
        
        #endregion

        public bool DisplayDurability;
        [SerializeField, Range(0, 1)] float durabilityValue;

        [Header("Elements")]
        [SerializeField] StaticGradient durabilityGradient;
        [SerializeField] Image image;
        [SerializeField] Image durabilityBar;
        [SerializeField] TextMeshProUGUI altText;
        [SerializeField] TextMeshProUGUI countText;

        void OnEnable()
        {
            image.preserveAspect = true;
        }

        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                base.OnValidate();

                durabilityBar.enabled = DisplayDurability;
                durabilityBar.fillAmount = durabilityValue;
                durabilityBar.color = durabilityGradient.Gradient.Evaluate(durabilityValue);
            }
        }

        void OnDestroy()
        {
            RemoveDurabilityCallback();
        }

        void RemoveDurabilityCallback()
        {
            if (durability != null)
            {
                durability.OnValueChanged -= OnDurabilityChanged;
            }
        }

        void OnDurabilityChanged(object sender, AttributeValueChangedContext e)
        {
            SetDurability((sender as Attributes.Attribute).ValueMaxRatio);
        }

        #region Public methods

        public void SetDisplayedItem(Item item)
        {            
            this.item = item;
            
            if (item.IsNone)
            {
                image.sprite = null;
                image.color = ItemSlotUI.Transparent;
                countText.text = "";
                altText.enabled = false;
                RemoveDurabilityCallback();
            }
            else
            {
                if (item.Data.Sprite != null)
                {
                    image.color = ItemSlotUI.Opaque;
                    altText.enabled = false;
                    altText.text = string.Empty;
                }
                else
                {
                    image.color = ItemSlotUI.Transparent;
                    altText.enabled = true;
                    altText.text = item.Data.DisplayName;
                }
                image.sprite = item.Data.Sprite;

                if (item.Count <= 1)
                {
                    countText.text = string.Empty;
                }
                else
                {
                    countText.text = item.Count.ToString();
                }

                if (item.HasAttributes)
                {
                    if (item.Attributes.TryGet("durability", out durability))
                    {
                        durability.OnValueChanged += OnDurabilityChanged;
                        SetDurability(durability.ValueMaxRatio);
                    }
                }
                else
                {
                    RemoveDurabilityCallback();
                }
            }
        }

        public void SetCountDisplayed(bool display)
        {
            countText.gameObject.SetActive(display);
        }

        public void SetDurability(float value)
        {
            value = Mathf.Clamp01(value);
            durabilityBar.fillAmount = value;
            durabilityBar.color = durabilityGradient.Gradient.Evaluate(value);
        }

        #endregion
    }
}