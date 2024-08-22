using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Objects;
using UZSG.Attributes;
using UZSG.Data;
using UnityEditor.Overlays;

namespace UZSG.UI
{
    public class ResourceHealthRingUI : Window
    {
        public float MaxValue;
        public float CurrentValue;
        [Range(0, 1)]
        [SerializeField] float progress;
        public float Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                Refresh();
            }
        }
        public string Label
        {
            get
            {
                return labelText.text;
            }
            set
            {
                labelText.text = value;
            }
        }
        public Sprite Icon
        {
            get
            {
                return icon.sprite;
            }
            set
            {
                icon.sprite = value;
            }
        }

        Attributes.Attribute health;

        [SerializeField] Image fill;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] TextMeshProUGUI labelText;
        [SerializeField] Image icon;

        void OnValidate()
        {
            Refresh();
        }

        void OnDestroy()
        {
            health.OnValueChanged -= OnHealthChanged;
        }

        void OnHealthChanged(object sender, AttributeValueChangedContext e)
        {
            Progress = ((Attributes.Attribute) sender).ValueMaxRatio;
        }

        Sprite GetIconFromToolType(ToolType type)
        {
            return null;
        }

        public void Refresh()
        {
            fill.fillAmount = progress;
            valueText.text = $"{Mathf.FloorToInt(MaxValue * progress)}";
        }

        public void DisplayResource(Resource resource)
        {
            Label = resource.ResourceData.Name;
            Icon = GetIconFromToolType(resource.ResourceData.ToolType);

            if (resource.Attributes.TryGet("health", out health))
            {
                MaxValue = health.CurrentMaximum;
                CurrentValue = health.Value;
                Progress = health.ValueMaxRatio;
                health.OnValueChanged += OnHealthChanged;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        }
    }
}