using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UZSG.Objects;
using UZSG.Attributes;
using UZSG.Data;
using UnityEditor.Overlays;
using UZSG.Systems;

namespace UZSG.UI
{
    public class ResourceHealthRingUI : UIElement
    {
        public float MaxValue;
        public float CurrentValue;
        [Range(0, 1)]
        [SerializeField] float progress;
        public float Progress
        {
            get => progress;
            set
            {
                progress = value;
                Refresh();
            }
        }
        public string Label
        {
            get => labelText.text;
            set => labelText.text = value;
        }
        public Sprite Icon
        {
            get => icon.sprite;
            set => icon.sprite = value;
        }

        Attributes.Attribute health;

        [SerializeField] GameObject healthRing;
        [SerializeField] Image fill;
        [SerializeField] TextMeshProUGUI valueText;
        [SerializeField] TextMeshProUGUI labelText;
        [SerializeField] Image icon;

        protected override void OnValidate()
        {
            base.OnValidate();
            Refresh();
        }

        void OnDestroy()
        {
            if (health != null)
            {
                health.OnValueChanged -= OnHealthChanged;
            }
        }

        void OnHealthChanged(object sender, AttributeValueChangedContext e)
        {
            Progress = ((Attributes.Attribute) sender).ValueMaxRatio;
        }

        Sprite GetToolTypeIcon(ToolType type)
        {
            var name = type switch
            {
                ToolType.Any => "tool_any",
                ToolType.Axe => "tool_axe",
                ToolType.Pickaxe => "tool_pickaxe",
                ToolType.Shovel => "tool_shovel",
                _ => "hand"
            };

            return Game.UI.GetIcon(name);
        }

        public void Refresh()
        {
            fill.fillAmount = progress;
            valueText.text = $"{Mathf.FloorToInt(MaxValue * progress)}";
        }

        public void SetHealthRingVisible(bool visible)
        {
            healthRing.gameObject.SetActive(visible);
        }

        public void DisplayResource(Resource resource)
        {
            Label = resource.ResourceData.Name;
            Icon = GetToolTypeIcon(resource.ResourceData.ToolType);

            if (resource.Attributes.TryGet("health", out health))
            {
                MaxValue = health.CurrentMaximum;
                CurrentValue = health.Value;
                Progress = health.ValueMaxRatio;
                health.OnValueChanged += OnHealthChanged;
            }

            Rebuild();
        }
    }
}